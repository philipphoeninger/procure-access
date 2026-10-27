// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { render, screen } from '@testing-library/angular';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { Product } from '../models/product.model';
import { ProductDetails } from './product-details';

// Covers the website link only (S1-09 review fix); the screen is rebuilt in a later step.
async function renderWithLink(link: string | null) {
  const product = new Product(7, 'Beispielprodukt', 1, link as string);
  await render(ProductDetails, {
    providers: [
      provideTestTranslations(),
      { provide: ProcureAccessStore, useValue: { getProductById: () => product } },
      { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ id: '7' }) } } },
      { provide: SnackbarService, useValue: {} },
    ],
  });
}

describe('ProductDetails', () => {
  it('adds https:// to a link without a scheme', async () => {
    await renderWithLink('example.org');

    expect(screen.getByRole('link', { name: 'example.org' }).getAttribute('href')).toBe('https://example.org');
  });

  it('keeps a link that already has a scheme', async () => {
    await renderWithLink('http://example.org/produkt');

    expect(screen.getByRole('link', { name: 'http://example.org/produkt' }).getAttribute('href'))
      .toBe('http://example.org/produkt');
  });

  it('shows no website link when the product has none', async () => {
    await renderWithLink(null);

    expect(screen.getByRole('heading', { name: 'Beispielprodukt' })).toBeTruthy();
    expect(screen.queryByRole('link')).toBeNull();
    expect(screen.queryByText(/Webseite/)).toBeNull();
  });
});
