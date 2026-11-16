// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';

import { Criterion } from '../../models/criteria';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';
import { decisionOf } from '../../state/criteria-list-rules';
import { provideLoadedConfiguratorSettings } from '../../testing/configurator-settings-stub';
import { criterion } from '../../testing/criteria-fixture';
import { NOTE_SAVE_DELAY_MS, SavedListEditor, toDecisionRequest } from './saved-list-editor';

const A = criterion('1.1', 'A', null, { revisionId: 11 });
const B = criterion('2.2', 'B', 3, { revisionId: 22 });

describe('SavedListEditor (R-SAV-03; owner decision S5-21b 1a)', () => {
  let decide: ReturnType<typeof vi.fn>;
  let editor: SavedListEditor;

  function setUp(criteria: Criterion[] = [A, B]) {
    decide = vi.fn().mockResolvedValue(null);
    TestBed.configureTestingModule({
      providers: [
        SavedListEditor,
        { provide: ConfiguratorApiService, useValue: { decide } },
        provideLoadedConfiguratorSettings(),
      ],
    });
    editor = TestBed.inject(SavedListEditor);
    editor.start('list-1', criteria, {});
  }

  const settle = () => new Promise((resolve) => setTimeout(resolve));

  afterEach(() => vi.useRealTimers());

  it('saves the changes of a criterion one after the other, each with the decision of that moment', async () => {
    setUp();
    let answer: (value: unknown) => void = () => undefined;
    decide.mockReturnValueOnce(new Promise((resolve) => (answer = resolve)));

    editor.setIncluded('1.1', false);
    await settle();
    editor.overridePriority('1.1', 'B');
    editor.setWeight('1.1', 7);
    answer(null);
    await settle();

    // The second save starts after the first answered and carries the newest decision; the third
    // finds nothing left to save.
    expect(decide.mock.calls).toEqual([
      ['list-1', 11, { isIncluded: false, overrideLevel: null, overrideWeight: null, note: null }],
      ['list-1', 11, { isIncluded: false, overrideLevel: 'B', overrideWeight: 7, note: null }],
    ]);
    expect(editor.decisions()['1.1']).toEqual({ included: false, overrideLevel: 'B', overrideWeight: 7, note: '' });
  });

  it('puts a criterion back to its last saved decision when the save fails', async () => {
    setUp();
    decide.mockRejectedValueOnce(new Error('500'));

    editor.overridePriority('2.2', 'Info');
    await settle();

    expect(editor.decisions()['2.2']).toEqual({ included: true, overrideLevel: null, overrideWeight: null, note: '' });
  });

  it('keeps a newer change when an older save of the same criterion fails', async () => {
    setUp();
    let reject: (reason: unknown) => void = () => undefined;
    decide.mockReturnValueOnce(new Promise((_, failed) => (reject = failed)));

    editor.setIncluded('1.1', false);
    await settle();
    editor.setIncluded('1.1', true);
    reject(new Error('500'));
    await settle();

    expect(decisionOf(editor.decisions(), '1.1').included).toBe(true);
  });

  it('saves a note after a pause in typing, once, and not while it is too long', async () => {
    vi.useFakeTimers();
    setUp();

    editor.setNote('1.1', 'Mit');
    editor.setNote('1.1', 'Mit Anbieter');
    await vi.advanceTimersByTimeAsync(NOTE_SAVE_DELAY_MS - 1);
    expect(decide).not.toHaveBeenCalled();
    await vi.advanceTimersByTimeAsync(1);
    expect(decide).toHaveBeenCalledExactlyOnceWith('list-1', 11, {
      isIncluded: true,
      overrideLevel: null,
      overrideWeight: null,
      note: 'Mit Anbieter',
    });

    editor.setNote('1.1', 'x'.repeat(1001));
    await vi.advanceTimersByTimeAsync(NOTE_SAVE_DELAY_MS);
    expect(decide).toHaveBeenCalledTimes(1);
  });

  it('saves a waiting note at once when the page is left', async () => {
    vi.useFakeTimers();
    setUp();

    editor.setNote('2.2', 'Prüfen');
    editor.flush();
    await vi.advanceTimersByTimeAsync(0);

    expect(decide).toHaveBeenCalledExactlyOnceWith('list-1', 22, {
      isIncluded: true,
      overrideLevel: null,
      overrideWeight: null,
      note: 'Prüfen',
    });
  });
});

describe('SavedListEditor.settled', () => {
  it('saves a waiting note and resolves once every save has answered', async () => {
    let answer: (value: unknown) => void = () => undefined;
    const decide = vi.fn().mockReturnValueOnce(new Promise((resolve) => (answer = resolve)));
    TestBed.configureTestingModule({
      providers: [
        SavedListEditor,
        { provide: ConfiguratorApiService, useValue: { decide } },
        provideLoadedConfiguratorSettings(),
      ],
    });
    const editor = TestBed.inject(SavedListEditor);
    editor.start('list-1', [A], {});
    let done = false;

    editor.setNote('1.1', 'Prüfen');
    const settled = editor.settled().then(() => (done = true));
    await new Promise((resolve) => setTimeout(resolve));
    expect(decide).toHaveBeenCalledOnce();
    expect(done).toBe(false);

    answer(null);
    await settled;
    expect(done).toBe(true);
  });
});

describe('toDecisionRequest', () => {
  it('sends the weight only while B applies and no empty note', () => {
    expect(toDecisionRequest(B, { included: true, overrideLevel: null, overrideWeight: 8, note: '' })).toEqual({
      isIncluded: true,
      overrideLevel: null,
      overrideWeight: 8,
      note: null,
    });
    expect(toDecisionRequest(B, { included: true, overrideLevel: 'A', overrideWeight: 8, note: 'x' })).toEqual({
      isIncluded: true,
      overrideLevel: 'A',
      overrideWeight: null,
      note: 'x',
    });
  });
});
