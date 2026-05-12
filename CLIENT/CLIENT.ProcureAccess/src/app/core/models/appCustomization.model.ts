export class AppCustomization {
    foregroundColor: string;
    backgroundColor: string;
    textColor: string;
    highContrastEnabled: boolean;

    constructor(
        pForegroundColor: string, 
        pBackgroundColor: string, 
        pTextColor: string, 
        pHighContrastEnabled: boolean
    ) {
        this.foregroundColor = pForegroundColor;
        this.backgroundColor = pBackgroundColor;
        this.textColor = pTextColor;
        this.highContrastEnabled = pHighContrastEnabled;
    }
}
