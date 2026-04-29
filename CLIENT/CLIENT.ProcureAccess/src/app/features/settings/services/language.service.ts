import { inject, Injectable } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
import { EnLanguage } from "@core/models/language.enum";
import { DEFAULT_LANGUAGE } from "@app/app.config";

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private translate = inject(TranslateService);

  constructor() {
    // this.translate.onLangChange.subscribe(res => {
    //   console.log(res);
    // });
  }

  init(pLang: EnLanguage) {
    // const browser = navigator.language.split('-')[0];
    // const lang = pLang || browser || this.translate.getFallbackLang()!;
    const langs = Object.values(EnLanguage);
    this.translate.addLangs(langs);
    this.translate.setFallbackLang(DEFAULT_LANGUAGE);
    this.set(pLang);
  }

  set(lang: EnLanguage) {
    this.translate.use(lang);
  }

  get(): EnLanguage {
    let currentLang = this.translate.getCurrentLang();
    let validLang = EnLanguage[currentLang];
    return validLang || DEFAULT_LANGUAGE;
  }
}
