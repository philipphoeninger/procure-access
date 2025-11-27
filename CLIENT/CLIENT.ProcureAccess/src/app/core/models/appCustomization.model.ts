export class AppCustomization {
    foregroundColor: string;
    backgroundColor: string;
    textColor: string;
    orientation: string;
    highContrastEnabled: boolean;

    constructor(
        pForegroundColor: string, 
        pBackgroundColor: string, 
        pTextColor: string, 
        pOrientation: string, 
        pHighContrastEnabled: boolean
    ) {
        this.foregroundColor = pForegroundColor;
        this.backgroundColor = pBackgroundColor;
        this.textColor = pTextColor;
        this.orientation = pOrientation;
        this.highContrastEnabled = pHighContrastEnabled;
    }
}