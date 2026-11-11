// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Component, input, OnInit, signal } from '@angular/core';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { render, screen, within } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { PriorityIndicator, PriorityWeightRange } from './priority-indicator';
import { PriorityLevel } from './priority-level';
import { PriorityRationale } from './priority-rationale';

const RANGE: PriorityWeightRange = { min: 1, max: 10 };

interface Setup {
  level: PriorityLevel;
  recommendedLevel?: PriorityLevel;
  weight?: number | null;
  recommendedWeight?: number | null;
  weightRange?: PriorityWeightRange | null;
}

/**
 * K5 under a criterion heading, as the S11 card places it: the host keeps level and weight like the
 * store does, so a choice comes back as the new input.
 */
@Component({
  imports: [PriorityIndicator],
  template: `
    <h2 id="k-title">Formulare lassen sich mit der Tastatur bedienen</h2>
    <pa-priority-indicator
      [(level)]="level"
      [(weight)]="weight"
      [recommendedLevel]="recommendedLevel()"
      [recommendedWeight]="recommendedWeight()"
      [weightRange]="weightRange()"
      criterionTitleId="k-title"
      (restoreRecommendation)="restore()"
    />
  `,
})
class Host implements OnInit {
  readonly initial = input.required<Setup>();
  readonly level = signal<PriorityLevel>('A');
  readonly weight = signal<number | null>(null);
  readonly recommendedLevel = signal<PriorityLevel>('A');
  readonly recommendedWeight = signal<number | null>(null);
  readonly weightRange = signal<PriorityWeightRange | null>(null);
  resets = 0;

  ngOnInit(): void {
    const setup = this.initial();
    this.level.set(setup.level);
    this.weight.set(setup.weight ?? null);
    this.recommendedLevel.set(setup.recommendedLevel ?? setup.level);
    this.recommendedWeight.set(setup.recommendedWeight ?? null);
    this.weightRange.set(setup.weightRange ?? null);
  }

  restore(): void {
    this.resets++;
    this.level.set(this.recommendedLevel());
    this.weight.set(this.recommendedWeight());
  }
}

describe('PriorityIndicator (K5, Handoff §4.5, R-LST-03)', () => {
  let announce: ReturnType<typeof vi.fn>;

  async function show(setup: Setup) {
    announce = vi.fn().mockResolvedValue(undefined);
    const rendered = await render(Host, {
      providers: [
        provideTestTranslations(),
        { provide: A11yAnnouncerService, useValue: { announce } },
        // jsdom never ends a CSS transition, so the menu would never finish closing.
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
      ],
      inputs: { initial: setup },
    });
    // mat-select shows its value once its options exist.
    await rendered.fixture.whenStable();
    const host = rendered.fixture.componentInstance;
    const levelButton = () => screen.getByRole('button', { name: /^Priorität:/ });
    async function act(element: HTMLElement): Promise<void> {
      element.click();
      await rendered.fixture.whenStable();
    }
    return { ...rendered, host, levelButton, act };
  }

  it.each([
    ['A', 'A – Mindestanforderung'],
    ['B', 'B – Bewertungskriterium'],
    ['Info', 'Hinweis'],
  ] as const)('shows level %s as a menu button „Priorität: …“, described by the criterion title', async (level, label) => {
    const { container, levelButton } = await show({ level });

    expect(levelButton().textContent).toContain(label);
    expect(screen.getByText(`Priorität: ${label}`).classList).toContain('cdk-visually-hidden');
    expect(levelButton().getAttribute('aria-haspopup')).toBe('menu');
    expect(levelButton().getAttribute('aria-describedby')).toBe('k-title');
    expect(container.textContent).not.toContain('Von Ihnen geändert');
    await expectNoAxeViolations();
  });

  it('opens the menu with one radio item per level, the current one checked', async () => {
    const { levelButton, act } = await show({ level: 'B', weight: 3, weightRange: RANGE });

    await act(levelButton());

    const menu = screen.getByRole('menu', { name: 'Priorität für dieses Kriterium' });
    expect(within(menu).getAllByRole('menuitemradio')).toHaveLength(3);
    const checked = (name: string) =>
      within(menu).getByRole('menuitemradio', { name }).getAttribute('aria-checked');
    expect(checked('A – Mindestanforderung')).toBe('false');
    expect(checked('B – Bewertungskriterium')).toBe('true');
    expect(checked('Hinweis')).toBe('false');
    await expectNoAxeViolations();
  });

  it('changes the level from the menu, returns the focus and keeps the recommendation visible (CAT-08 on the client)', async () => {
    const { host, levelButton, act } = await show({ level: 'A' });
    await act(levelButton());

    await act(screen.getByRole('menuitemradio', { name: /Hinweis/ }));

    expect(host.level()).toBe('Info');
    expect(host.recommendedLevel()).toBe('A');
    expect(screen.queryByRole('menu')).toBeNull();
    expect(document.activeElement).toBe(levelButton());
    expect(levelButton().textContent).toContain('Hinweis');
    expect(announce).toHaveBeenCalledWith('criteria.priority.changed', { level: 'Hinweis' });
    expect(screen.getByText('Von Ihnen geändert. Empfohlen war: A – Mindestanforderung.')).toBeTruthy();
    await expectNoAxeViolations();
  });

  it('changes nothing and announces nothing when the current level is chosen again', async () => {
    const { host, levelButton, act } = await show({ level: 'A' });
    await act(levelButton());

    await act(screen.getByRole('menuitemradio', { name: /Mindestanforderung/ }));

    expect(host.level()).toBe('A');
    expect(announce).not.toHaveBeenCalled();
  });

  it('restores the recommendation with the focus on the level button and announces it', async () => {
    const { host, levelButton, act } = await show({ level: 'Info', recommendedLevel: 'A' });
    const reset = screen.getByRole('button', { name: 'Empfehlung wiederherstellen' });
    expect(reset.getAttribute('aria-describedby')).toBe('k-title');

    await act(reset);

    expect(host.resets).toBe(1);
    expect(screen.queryByRole('button', { name: 'Empfehlung wiederherstellen' })).toBeNull();
    expect(screen.queryByText(/Von Ihnen geändert/)).toBeNull();
    expect(document.activeElement).toBe(levelButton());
    expect(announce).toHaveBeenCalledWith('criteria.priority.changed', { level: 'A – Mindestanforderung' });
  });

  describe('weight (O-S4-40, O-S4-41)', () => {
    it('offers the weight field „Gewichtung“ with the range while B applies', async () => {
      const { levelButton, act } = await show({ level: 'B', weight: 3, recommendedWeight: 3, weightRange: RANGE });
      const field = screen.getByRole('combobox', { name: 'Gewichtung' });

      expect(field.getAttribute('aria-describedby')).toContain('k-title');
      expect(field.textContent).toContain('3');
      expect(levelButton().textContent).not.toContain('Gewichtung');
      await act(field);
      expect(screen.getAllByRole('option').map((option) => option.textContent?.trim())).toEqual(
        ['1', '2', '3', '4', '5', '6', '7', '8', '9', '10'],
      );
      await expectNoAxeViolations();
    });

    it('sets the chosen weight and shows the recommended one next to it', async () => {
      const { host, act } = await show({ level: 'B', weight: 3, recommendedWeight: 3, weightRange: RANGE });
      await act(screen.getByRole('combobox', { name: 'Gewichtung' }));

      await act(screen.getByRole('option', { name: '7' }));

      expect(host.weight()).toBe(7);
      expect(
        screen.getByText('Von Ihnen geändert. Empfohlen war: B – Bewertungskriterium, Gewichtung 3.'),
      ).toBeTruthy();
      expect(screen.getByRole('button', { name: 'Empfehlung wiederherstellen' })).toBeTruthy();
      await expectNoAxeViolations();
    });

    it('names no recommended weight when the recommended B has none', async () => {
      await show({ level: 'B', weight: 5, recommendedWeight: null, weightRange: RANGE });

      expect(screen.getByText('Von Ihnen geändert. Empfohlen war: B – Bewertungskriterium.')).toBeTruthy();
    });

    it('starts empty after an override to B from a level without weight (O-S4-27)', async () => {
      await show({ level: 'B', recommendedLevel: 'A', weightRange: RANGE });

      expect(screen.getByRole('combobox', { name: 'Gewichtung' }).textContent?.trim()).toBe('');
      expect(screen.getByText('Von Ihnen geändert. Empfohlen war: A – Mindestanforderung.')).toBeTruthy();
    });

    it('has no weight field for a level other than B', async () => {
      const { container } = await show({ level: 'A', weight: 3, weightRange: RANGE });

      expect(screen.queryByRole('combobox')).toBeNull();
      expect(container.textContent).not.toContain('Gewichtung');
    });

    it('shows the weight in the level button when no range makes it editable', async () => {
      const { levelButton } = await show({ level: 'B', weight: 3, recommendedWeight: 3 });

      expect(screen.queryByRole('combobox')).toBeNull();
      expect(levelButton().textContent).toContain('Gewichtung 3');
    });
  });
});

describe('PriorityRationale (K5 panel, CAT-02)', () => {
  it('INV:CAT-02 shows rationale, matched rule and source „Empfehlung des Projektteams“', async () => {
    await render(PriorityRationale, {
      providers: [provideTestTranslations()],
      inputs: {
        rationale: 'Viele Mitarbeitende arbeiten nur mit der Tastatur.',
        rule: { key: 'criteria.priority.rule.full', params: { appType: 'Webanwendung', useCase: 'HR-Anwendung' } },
      },
    });

    expect(screen.getByText('Viele Mitarbeitende arbeiten nur mit der Tastatur.')).toBeTruthy();
    expect(screen.getByText('Diese Empfehlung gilt für Webanwendung mit dem Verwendungszweck HR-Anwendung.')).toBeTruthy();
    expect(
      screen.getByText(
        'Grundlage: Empfehlung des Projektteams WERTE.IT (fachliche Einschätzung, keine Vorgabe aus einer Norm).',
      ),
    ).toBeTruthy();
    await expectNoAxeViolations();
  });
});
