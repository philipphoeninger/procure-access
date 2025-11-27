import { AppCustomization } from "./appCustomization.model";
import { IdentityState } from "@app/features/identity/state/with-identity";

export interface AppState {
    loadingCount: number;
    appConfiguration: AppCustomization;
    identity: IdentityState;
}

export const initialAppState: AppState = {
    loadingCount: 0,
    appConfiguration: {
        foregroundColor: '',
        backgroundColor: '',
        textColor: '',
        orientation: '',
        highContrastEnabled: false
    },
    identity: {
        user: null
    }
}