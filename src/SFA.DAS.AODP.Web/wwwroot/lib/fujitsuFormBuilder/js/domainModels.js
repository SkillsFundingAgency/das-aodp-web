//###########################################
//Domain Models
class Form {
    constructor(data) {
        this.id = data.id;
        this.version = data.version;
        this.published = data.published;
        this.name = data.name;
        this.key = data.key;
        this.applicationTrackingTemplate = data.applicationTrackingTemplate;
        this.sections = data.sections ? data.sections.map(sectionData => new Section(this, sectionData)) : [];
    }
}

class Section {
    constructor(form, data) {
        this.id = data.id;
        this.formId = form.id; // Reference to the parent form
        this.order = data.order;
        this.title = data.title;
        this.description = data.description;
        this.nextSectionId = data.nextSectionId;
        this.pages = data.pages ? data.pages.map(pageData => new Page(this, pageData)) : [];
    }
}

class Page {
    constructor(section, data) {
        this.id = data.id;
        this.sectionId = section.id; // Reference to the parent section
        this.title = data.title;
        this.description = data.description;
        this.order = data.order;
        this.nextPageId = data.nextPageId;
        this.questions = data.questions ? data.questions.map(questionData => new Question(this, questionData)) : [];
    }
}

class Question {
    constructor(page, data) {
        this.id = data.id || generateUUID(); // Generate ID if not provided
        this.pageId = page.id; // Reference to the parent page
        this.title = data.title;
        this.type = data.type;
        this.required = data.required;
        this.order = data.order;
        this.description = data.description;
        this.hint = data.hint;
        this.options = data.options ? data.options.map(optionData => new Option(this, optionData)) : [];
        this.validationRules = data.validationRules ? data.validationRules.map(ruleData => new ValidationRule(this, ruleData)) : [];
        this.navigationRules = data.navigationRules ? data.navigationRules.map(ruleData => new NavigationRule(this, ruleData)) : [];
    }
}

class Option {
    constructor(question, data) {
        this.id = data.id || generateUUID(); // Generate ID if not provided
        this.questionId = question.id; // Reference to the parent question
        this.value = data.value;
        this.text = data.text;
        this.order = data.order;
    }
}

class ValidationRule {
    constructor(question, data) {
        this.id = data.id || generateUUID(); // Generate ID if not provided
        this.questionId = question.id; // Reference to the parent question
        this.type = data.type;
        this.value = data.value;
        this.errorMessage = data.errorMessage;
    }
}

class NavigationRule {
    constructor(question, data) {
        this.id = data.id || generateUUID(); // Generate ID if not provided
        this.pageId = data.pageId;
        this.sourceQuestionId = data.sourceQuestionId;
        this.operator = data.operator;
        this.value = data.value;
        this.targetSectionId = data.targetSectionId;
        this.targetPageId = data.targetPageId;
    }
}
//###########################################

// utils.js
function generateUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}