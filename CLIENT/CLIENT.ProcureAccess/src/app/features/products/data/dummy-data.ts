import { AppType } from "../models/appType.model";
import { Product } from "../models/product.model";
import { ProductPart } from "../models/productPart.model";
import { TestType } from "../models/testType.model";
import { ProductType } from "../models/productType.model";

export const products: Product[] = [
    {
        id: 0,
        name: "CoreFlow",
        link: "www.coreflow.org",
        functionality: "A short description for...",
        type: "website/webapp"
    },
    {
        id: 1,
        name: "IntraSphere",
        link: "www.intrasphere.com",
        functionality: "",
        type: "hardware"
    },
    {
        id: 2,
        name: "ProximaSuite",
        link: "www.proximasuite.com",
        functionality: "",
        type: "dispatch/emergency service"
    },
    {
        id: 3,
        name: "WorkBridge",
        link: "www.workbridge.com",
        functionality: "",
        type: "software (closed-source)"
    },
    {
        id: 4,
        name: "SyntraLogic",
        link: "www.syntralogic.it",
        functionality: "",
        type: "mobile app (open-source)"
    },
    {
        id: 5,
        name: "Nexaro",
        link: "www.nexaro.de",
        functionality: "Another long description...",
        type: "website/webapp"
    },
    {
        id: 6,
        name: "FlowMatrix",
        link: "www.flowmatrix.com",
        functionality: "",
        type: "document"
    },
    {
        id: 7,
        name: "OptiCore",
        link: "www.opticore.com",
        functionality: "",
        type: "dispatch/emergency service"
    },
    {
        id: 8,
        name: "BizStream",
        link: "www.bizstream.ru",
        functionality: "",
        type: "website/webapp"
    },
    {
        id: 9,
        name: "NovaDesk",
        link: "www.novadesk.at",
        functionality: "",
        type: "website/webapp"
    },
    {
        id: 10,
        name: "ClarioOne",
        link: "www.clarioone.fr",
        functionality: "",
        type: "website/webapp"
    },
    {
        id: 11,
        name: "FlexiSuite",
        link: "www.flexisuite.com",
        functionality: "",
        type: "mobile app (closed-source)"
    },
    {
        id: 13,
        name: "EvoManage",
        link: "www.evomanage.com",
        functionality: "",
        type: "software (open-source)"
    },
    {
        id: 14,
        name: "TaskFusion",
        link: "www.taskfusion.com",
        functionality: "",
        type: ""
    },
    {
        id: 15,
        name: "PrimeHub",
        link: "www.primehub.org",
        functionality: "",
        type: "website/webapp"
    },
    {
        id: 16,
        name: "WorklineOS",
        link: "www.worklineos.de",
        functionality: "",
        type: "hardware"
    },
    {
        id: 17,
        name: "AquilaWare",
        link: "www.aquilaware.com",
        functionality: "",
        type: "document"
    }
];

export const appTypes: AppType[] = [
    {
        id: 0,
        name: "Website / Webapp"
    },
    {
        id: 1,
        name: "Software (open-source)"
    },
    {
        id: 2,
        name: "Software (closed-source)"
    },
    {
        id: 3,
        name: "Mobile App (open-source)"
    },
    {
        id: 4,
        name: "Mobile App (closed-source)"
    },
    {
        id: 5,
        name: "Document"
    },
    {
        id: 6,
        name: "Hardware"
    },
    {
        id: 7,
        name: "Dispatch / Emergency Service"
    }
]

export const productParts: ProductPart[] = [
    {
        id: 0,
        name: "Core functionality"
    },
    {
        id: 1,
        name: "Audio"
    },
    {
        id: 2,
        name: "Video"
    },
    {
        id: 3,
        name: "Two-way voice communication"
    },
    {
        id: 4,
        name: "Real-time text"
    },
    {
        id: 5,
        name: "Biometrics"
    },
    {
        id: 6,
        name: "Authoring tools"
    }
];

export const testTypes: TestType[] = [
    {
        id: 0,
        name: "BITV"
    },
    {
        id: 1,
        name: "Automated"
    },
    {
        id: 2,
        name: "In-Person"
    },
    {
        id: 3,
        name: "Manual"
    }
];

export const productTypes: ProductType[] = [
    {
        id: 0,
        name: "Datenbankgestützte Fachanwendung"
    },
    {
        id: 1,
        name: "Human Resources"
    },
    {
        id: 2,
        name: "Reporting"
    },
    {
        id: 3,
        name: "Fortbildungsmanagement"
    },
    {
        id: 4,
        name: "Dienstreiseanträge"
    },
    {
        id: 5,
        name: "Gehaltsabrechnungen"
    }
];
