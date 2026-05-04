import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';
import { EnLanguage } from '@core/models/language.enum';

@jsonObject
export class UICustomization {
  @jsonMember
  foregroundColor: string;

  @jsonMember
  backgroundColor: string;

  @jsonMember
  textColor: string;

  @jsonMember
  darkModeOn: boolean;

  @jsonMember
  orientationVertical: boolean;

  @jsonMember
  highContrastOn: boolean;

  @jsonMember
  language: EnLanguage;

  constructor(
    pForegroundColor: string,
    pBackgroundColor: string,
    pTextColor: string,
    pDarkModeOn: boolean,
    pOrientationVertical: boolean,
    pHighContrastOn: boolean,
    pLanguage: EnLanguage,
  ) {
    this.foregroundColor = pForegroundColor;
    this.backgroundColor = pBackgroundColor;
    this.textColor = pTextColor;
    this.darkModeOn = pDarkModeOn;
    this.orientationVertical = pOrientationVertical;
    this.highContrastOn = pHighContrastOn;
    this.language = pLanguage;
  }
}

