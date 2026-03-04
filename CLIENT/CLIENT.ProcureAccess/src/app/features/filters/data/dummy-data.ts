import { Criterion } from "@app/features/criteria/models/criterion.model";
import { FilterType } from "../models/filterType.model";
import { CriteriaFilter } from "../models/criteriaFilter.model";

export const appTypeCriteria1: Criterion[] = [
    {
        id: 0,
        name: "Orientation",
        description: "Content does not restrict its view and operation to a single display orientation, such as portrait or landscape, unless a specific display orientation is essential.\nThe objective of this technique is to allow users to access content in the way the user prefers. A content provider may expect that most users will view content using a specific device orientation or may expect that a user will want to maintain the original view within the device. As a result the provider then prevents the content from rotating. By providing a control to allow the user to rotate the content, someone who needs to use a particular orientation will be able to view the content in a comfortable manner."
    },
    {
        id: 1,
        name: "Tab-Flow",
        description: "All functionality of the content is operable through a keyboard interface without requiring specific timings for individual keystrokes.\nIf keyboard focus can be moved to a component of the page using a keyboard interface, then focus can be moved away from that component using only a keyboard interface, and, if it requires more than unmodified arrow or tab keys or other standard exit methods, the user is advised of the method for moving focus away."
    }
];


export const appTypeCriteria2: Criterion[] = [
    {
        id: 2,
        name: "Resize Text",
        description: "Except for captions and images of text, text can be resized without assistive technology up to 200 percent without loss of content or functionality.\nThe objective of this technique is to ensure content can be scaled uniformly by using a web technology supported by user agents that change text size via a Zoom tool.\nContent authored in technologies that are supported by user agents that can scale content uniformly (that is, zoom into content) satisfy this success criterion. Because this technique relies completely on user agent functionality, it is critical to test with a wide variety of user agents.\nThis technique requires that the zoom function preserve all spatial relationships on the page and that all functionality continues to be available."
    },
    {
        id: 3,
        name: "Images of Text",
        description: "If the technologies being used can achieve the visual presentation, text is used to convey information rather than images of text except for the following:\n- Customizable: The image of text can be visually customized to the user's requirements;\n- Essential: A particular presentation of text is essential to the information being conveyed."
    }
];

export const filterTypes: FilterType[] = [
    {
        id: 0,
        name: "Product Types",
        description: "What product is being developed or purchased?",
    },
    {
        id: 1,
        name: "Application Types",
        description: "What type of IT solution is being developed or purchased?"
    },
    {
        id: 2,
        name: "Product Parts",
        description: "Which of the following characteristics apply?"
    },
    {
        id: 3,
        name: "Test Types",
        description: "Which accessibility tests should the products at least have been tested on?"
    }
];

export const productTypeCriteriaFilters: CriteriaFilter[] = [
    {
        id: 0,
        name: "Datenbankgestützte Fachanwendung",
        description: "",
        filterType: filterTypes[0],
        criteria: []
    },
    {
        id: 1,
        name: "Human Resources",
        description: "",
        filterType: filterTypes[0],
        criteria: []
    },
    {
        id: 2,
        name: "Reporting",
        description: "",
        filterType: filterTypes[0],
        criteria: []
    },
    {
        id: 3,
        name: "Fortbildungsmanagement",
        description: "",
        filterType: filterTypes[0],
        criteria: []
    },
    {
        id: 4,
        name: "Dienstreiseanträge",
        description: "",
        filterType: filterTypes[0],
        criteria: []
    },
    {
        id: 5,
        name: "Gehaltsabrechnungen",
        description: "",
        filterType: filterTypes[0],
        criteria: []
    }
];

export const appTypeCriteriaFilters: CriteriaFilter[] = [
    {
        id: 6,
        name: "Website / Webapp",
        description: "",
        filterType: filterTypes[1],
        criteria: []
        // criteria: appTypeFilterTypeValue6Criteria
    },
    {
        id: 7,
        name: "Software (open-source)",
        description: "",
        filterType: filterTypes[1],
        criteria: []
        // criteria: appTypeFilterTypeValue7Criteria
    },
    {
        id: 8,
        name: "Software (closed-source)",
        description: "",
        filterType: filterTypes[1],
        criteria: []
    },
    {
        id: 9,
        name: "Mobile App (open-source)",
        description: "",
        filterType: filterTypes[1],
        criteria: []
    },
    {
        id: 10,
        name: "Mobile App (closed-source)",
        description: "",
        filterType: filterTypes[1],
        criteria: []
    },
    {
        id: 11,
        name: "Document",
        description: "",
        filterType: filterTypes[1],
        criteria: []
    },
    {
        id: 12,
        name: "Hardware",
        description: "",
        filterType: filterTypes[1],
        criteria: []
    },
    {
        id: 13,
        name: "Dispatch / Emergency Service",
        description: "",
        filterType: filterTypes[1],
        criteria: []
    }
];

export const productPartCriteriaFilters: CriteriaFilter[] = [
    {
        id: 14,
        name: "Core functionality",
        description: "",
        filterType: filterTypes[2],
        criteria: []
    },
    {
        id: 15,
        name: "Audio",
        description: "",
        filterType: filterTypes[2],
        criteria: []
    },
    {
        id: 16,
        name: "Video",
        description: "",
        filterType: filterTypes[2],
        criteria: []
    },
    {
        id: 17,
        name: "Two-way voice communication",
        description: "",
        filterType: filterTypes[2],
        criteria: []
    },
    {
        id: 18,
        name: "Real-time text",
        description: "",
        filterType: filterTypes[2],
        criteria: []
    },
    {
        id: 19,
        name: "Biometrics",
        description: "",
        filterType: filterTypes[2],
        criteria: []
    },
    {
        id: 20,
        name: "Authoring tools",
        description: "",
        filterType: filterTypes[2],
        criteria: []
    }
];


export const testTypeCriteriaFilters: CriteriaFilter[] = [
    {
        id: 21,
        name: "BITV",
        description: "",
        filterType: filterTypes[3],
        criteria: []
    },
    {
        id: 22,
        name: "Automated",
        description: "",
        filterType: filterTypes[3],
        criteria: []
    },
    {
        id: 23,
        name: "In-Person",
        description: "",
        filterType: filterTypes[3],
        criteria: []
    },
    {
        id: 24,
        name: "Manual",
        description: "",
        filterType: filterTypes[3],
        criteria: []
    }
];

export const criteriaFilters: CriteriaFilter[] = [
    ...productTypeCriteriaFilters,
    ...appTypeCriteriaFilters,
    ...productPartCriteriaFilters,
    ...testTypeCriteriaFilters
];

export const criteria: Criterion[] = [
    ...appTypeCriteria1,
    ...appTypeCriteria2
];
