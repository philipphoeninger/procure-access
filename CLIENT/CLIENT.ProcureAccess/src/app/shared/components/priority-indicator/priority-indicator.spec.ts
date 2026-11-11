// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { render, screen } from '@testing-library/angular';

import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { PriorityIndicator } from './priority-indicator';
import { PriorityLevel } from './priority-level';
import { PriorityRationale } from './priority-rationale';

describe('PriorityIndicator (K5, Handoff §4.5)', () => {
  async function show(level: PriorityLevel, recommendedLevel: PriorityLevel = level, weight: number | null = null) {
    const { container } = await render(PriorityIndicator, {
      providers: [provideTestTranslations()],
      inputs: { level, recommendedLevel, weight },
    });
    return container;
  }

  it.each([
    ['A', 'A – Mindestanforderung'],
    ['B', 'B – Bewertungskriterium'],
    ['Info', 'Hinweis'],
  ] as const)('shows level %s with the name „Priorität: …“ for screen readers', async (level, label) => {
    const container = await show(level);

    expect(screen.getByText(`Priorität: ${label}`).classList).toContain('cdk-visually-hidden');
    expect(screen.getByText(label).getAttribute('aria-hidden')).toBe('true');
    expect(container.textContent).not.toContain('Gewichtung');
    expect(container.textContent).not.toContain('Von Ihnen geändert');
    await expectNoAxeViolations();
  });

  it('shows the weight of B', async () => {
    await show('B', 'B', 3);

    expect(screen.getByText('Gewichtung 3')).toBeTruthy();
  });

  it('shows no weight for a level other than B, even when one is given', async () => {
    const container = await show('A', 'A', 3);

    expect(container.textContent).not.toContain('Gewichtung');
  });

  it('keeps the recommendation visible next to an override (CAT-08 follows with S4-16)', async () => {
    await show('A', 'B', null);

    expect(screen.getByText('Priorität: A – Mindestanforderung')).toBeTruthy();
    expect(screen.getByText('Von Ihnen geändert. Empfohlen war: B – Bewertungskriterium.')).toBeTruthy();
    await expectNoAxeViolations();
  });

  it('offers no button until the menu of S4-16 exists', async () => {
    await show('A');

    expect(screen.queryByRole('button')).toBeNull();
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
