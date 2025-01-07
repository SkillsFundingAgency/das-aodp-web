//
(function () {
    /**
     * Initializes the form builder.
     * @param {HTMLElement} container - The container element for the form builder.
     * @param {Object} options - Configuration options for the form builder.
     */
    function fujitsuFormBuilder(container, options) {
        const formData = {
            sections: []
        };

        const questionTemplates = {
            text: '<input type="text" class="govuk-input" name="questionText">',
            textarea: '<textarea name="questionTextarea"></textarea>',
            number: '<input type="number" name="questionNumber">',
            date: '<input type="date" name="questionDate">',
            checkbox: '',
            radio: ''
        };

        // Default options
        const defaultOptions = {
            dataSource: {
                endpoints: {
                    insert: '',
                    update: '',
                    read: ''
                },
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                events: {
                    onBeforeSend: function (instance) {
                        console.log('Default onBeforeSend', instance);
                    },
                    onSending: function (instance) {
                        console.log('Default onSending', instance);
                    },
                    onSent: function (response, instance) {
                        console.log('Default onSent', response, instance);
                    },
                    onError: function (error, instance) {
                        console.error('Default onError', error, instance);
                    },
                    onBeforeLoad: function (instance) {
                        console.log('Default onBeforeLoad', instance);
                    },
                    onLoading: function (instance) {
                        console.log('Default onLoading', instance);
                    },
                    onLoad: function (response, instance) {
                        console.log('Default onLoad', response, instance);
                        instance.loadFormData(response);
                    },
                    onLoadError: function (error, instance) {
                        console.error('Default onLoadError', error, instance);
                    }
                }
            },
            editing: {
                enabled: false,
                saveButton: {
                    title: 'Save',
                    cssClass: 'save-button',
                    events: {
                        onSave: function (instance, button) {
                            console.log('Default onSave', instance, button);
                        },
                        onSaving: function (instance, button) {
                            console.log('Default onSaving', instance, button);
                        },
                        onSaveSuccess: function (response, instance, button) {
                            console.log('Default onSaveSuccess', response, instance, button);
                        },
                        onSaveError: function (error, instance, button) {
                            console.error('Default onSaveError', error, instance, button);
                        }
                    }
                }
            }
        };

        // Merge provided options with defaults
        this.options = {
            ...defaultOptions,
            ...options,
            dataSource: {
                ...defaultOptions.dataSource,
                ...options.dataSource,
                events: {
                    ...defaultOptions.dataSource.events,
                    ...options.dataSource?.events
                }
            },
            editing: {
                ...defaultOptions.editing,
                ...options.editing,
                saveButton: {
                    ...defaultOptions.editing.saveButton,
                    ...options.editing?.saveButton,
                    events: {
                        ...defaultOptions.editing.saveButton.events,
                        ...options.editing?.saveButton?.events
                    }
                }
            }
        };

        const { dataSource, editing } = this.options;

        // Initialize the form builder UI
        container.innerHTML = `
            <div id="container">
                <div id="formBuilderContent" class="${options.previewEnabled ? '' : 'full-width'}">
                    <h2>Form Builder</h2>
                    <button id="addSectionBtn" class="govuk-button">Add Section</button>
                    <div id="sectionsContainer"></div>
                </div>
                ${options.previewEnabled ? `
                <div id="formPreview">
                    <h2>Form Preview</h2>
                    <div class="tabs">
                        ${options.previewOptions.showPreviewTab ? `<button class="tabButton govuk-button govuk-button--secondary" onclick="formBuilderInstance.showTab('preview')">Preview</button>` : ''}
                        ${options.previewOptions.showJSONSourceTab ? `<button class="tabButton govuk-button govuk-button--secondary" onclick="formBuilderInstance.showTab('json')">JSON Source</button>` : ''}
                    </div>
                    <div id="previewContainer" class="tabContent"></div>
                    <pre id="jsonContainer" class="tabContent" style="display: none;"></pre>
                </div>` : ''}
            </div>
        `;

        // Event listener for adding a new section
        document.getElementById('addSectionBtn').addEventListener('click', addSection);

        // Event delegation for handling collapsible and delete buttons
        container.addEventListener('click', function (event) {
            if (event.target.classList.contains('collapsible')) {
                toggleCollapsible(event.target);
            } else if (event.target.classList.contains('deleteButton')) {
                const type = event.target.dataset.type;
                const id = event.target.dataset.id;
                if (type === 'section') {
                    deleteSection(id);
                } else if (type === 'page') {
                    const sectionId = event.target.dataset.sectionId;
                    deletePage(sectionId, id);
                } else if (type === 'question') {
                    const sectionId = event.target.dataset.sectionId;
                    const pageId = event.target.dataset.pageId;
                    deleteQuestion(sectionId, pageId, id);
                } else if (type === 'option') {
                    const sectionId = event.target.dataset.sectionId;
                    const pageId = event.target.dataset.pageId;
                    const questionId = event.target.dataset.questionId;
                    deleteOption(sectionId, pageId, questionId, id);
                }
            }
        });

        // AJAX function to send data
        function sendData(data, button, endpoint) {
            dataSource.events.onBeforeSend(this);
            const xhr = new XMLHttpRequest();
            xhr.open(dataSource.method, endpoint, true);
            for (const header in dataSource.headers) {
                xhr.setRequestHeader(header, dataSource.headers[header]);
            }
            xhr.onreadystatechange = function () {
                if (xhr.readyState === 4) {
                    if (xhr.status >= 200 && xhr.status < 300) {
                        const response = JSON.parse(xhr.responseText);
                        dataSource.events.onSent(response, this);
                        editing.saveButton.events.onSaveSuccess(response, this, button);
                        mergeFormData(response);
                    } else {
                        dataSource.events.onError(xhr.statusText, this);
                        editing.saveButton.events.onSaveError(xhr.statusText, this, button);
                    }
                }
            }.bind(this);
            xhr.send(JSON.stringify(data));
            dataSource.events.onSending(this);
        }

        // AJAX function to load data
        function loadData() {
            dataSource.events.onBeforeLoad(this);
            const xhr = new XMLHttpRequest();
            xhr.open('GET', dataSource.endpoints.read, true);
            for (const header in dataSource.headers) {
                xhr.setRequestHeader(header, dataSource.headers[header]);
            }
            xhr.onreadystatechange = function () {
                if (xhr.readyState === 4) {
                    if (xhr.status >= 200 && xhr.status < 300) {
                        const response = JSON.parse(xhr.responseText);
                        dataSource.events.onLoad(response, this);
                    } else {
                        dataSource.events.onLoadError(xhr.statusText, this);
                    }
                }
            }.bind(this);
            xhr.send();
            dataSource.events.onLoading(this);
        }

        // Merge response data with formData
        function mergeFormData(response) {
            // Implement merging logic as needed
            Object.assign(formData, response);
        }

        // Load existing form data
        this.loadFormData = function (data) {
            Object.assign(formData, data);
            updateUIWithFormData();
        };

        // Update the UI with the loaded form data
        function updateUIWithFormData() {
            const sectionsContainer = document.getElementById('sectionsContainer');
            sectionsContainer.innerHTML = '';

            formData.sections.forEach(section => {
                const sectionElement = document.createElement('div');
                sectionElement.className = 'section';
                sectionElement.dataset.id = section.id;
                sectionElement.innerHTML = `
                    <button class="collapsible govuk-button govuk-button--secondary">Section</button>
                    <button class="deleteButton govuk-button govuk-button--warning" data-type="section" data-id="${section.id}" aria-label="Delete Section">X</button>
                    <div class="content">
                        <button class="govuk-button" onclick="formBuilderInstance.addPage(this, '${section.id}')">Add Page</button>
                        <div class="pagesContainer"></div>
                    </div>
                `;
                sectionsContainer.appendChild(sectionElement);

                section.pages.forEach(page => {
                    const pageElement = document.createElement('div');
                    pageElement.className = 'page';
                    pageElement.dataset.id = page.id;
                    pageElement.innerHTML = `
                        <button class="collapsible govuk-button govuk-button--secondary">Page</button>
                        <button class="deleteButton govuk-button govuk-button--warning" data-type="page" data-id="${page.id}" data-section-id="${section.id}" aria-label="Delete Page">X</button>
                        <div class="content">
                            <button class="govuk-button" onclick="formBuilderInstance.addQuestion(this, '${section.id}', '${page.id}')">Add Question</button>
                            <div class="questionsContainer"></div>
                        </div>
                    `;
                    sectionElement.querySelector('.pagesContainer').appendChild(pageElement);

                    page.questions.forEach(question => {
                        const questionElement = document.createElement('div');
                        questionElement.className = 'question';
                        questionElement.dataset.id = question.id;
                        questionElement.innerHTML = `
                            <button class="collapsible govuk-button govuk-button--secondary">Question</button>
                            <button class="deleteButton govuk-button govuk-button--warning" data-type="question" data-id="${question.id}" data-section-id="${section.id}" data-page-id="${page.id}" aria-label="Delete Question">X</button>
                            <div class="content">
                                <label>Text: <input type="text" name="questionText" value="${question.text}" oninput="formBuilderInstance.updateQuestionText('${section.id}', '${page.id}', '${question.id}', this.value)"></label>
                                <label>Description: <input type="text" name="questionDescription" value="${question.description}" oninput="formBuilderInstance.updateQuestionDescription('${section.id}', '${page.id}', '${question.id}', this.value)"></label>
                                <label>Hint: <input type="text" name="questionHint" value="${question.hint}" oninput="formBuilderInstance.updateQuestionHint('${section.id}', '${page.id}', '${question.id}', this.value)"></label>
                                <label>Type:
                                    <select name="questionType" onchange="formBuilderInstance.updateQuestionType('${section.id}', '${page.id}', '${question.id}', this.value, this)">
                                        <option value="text" ${question.type === 'text' ? 'selected' : ''}>Text</option>
                                        <option value="textarea" ${question.type === 'textarea' ? 'selected' : ''}>Text Area</option>
                                        <option value="number" ${question.type === 'number' ? 'selected' : ''}>Number</option>
                                        <option value="date" ${question.type === 'date' ? 'selected' : ''}>Date</option>
                                        <option value="checkbox" ${question.type === 'checkbox' ? 'selected' : ''}>Checkbox</option>
                                        <option value="radio" ${question.type === 'radio' ? 'selected' : ''}>Radio</option>
                                    </select>
                                </label>
                                <div class="optionsContainer"></div>
                            </div>
                        `;
                        pageElement.querySelector('.questionsContainer').appendChild(questionElement);

                        if (question.type === 'checkbox' || question.type === 'radio') {
                            const optionsContainer = questionElement.querySelector('.optionsContainer');
                            const addOptionBtn = document.createElement('button');
                            addOptionBtn.textContent = 'Add Option';
                            addOptionBtn.type = 'button';
                            addOptionBtn.classList.add('govuk-button');
                            addOptionBtn.onclick = () => formBuilderInstance.addOption(section.id, page.id, question.id, optionsContainer);
                            optionsContainer.appendChild(addOptionBtn);

                            question.options.forEach(option => {
                                const optionElement = document.createElement('div');
                                optionElement.className = 'option';
                                optionElement.dataset.id = option.id;
                                optionElement.innerHTML = `
                                    <label>Option: <input type="text" value="${option.text}" oninput="formBuilderInstance.updateOptionText('${section.id}', '${page.id}', '${question.id}', '${option.id}', this.value)"></label>
                                    <button class="deleteButton govuk-button govuk-button--warning" data-type="option" data-id="${option.id}" data-section-id="${section.id}" data-page-id="${page.id}" data-question-id="${question.id}" aria-label="Delete Option">X</button>
                                `;
                                optionsContainer.appendChild(optionElement);
                            });
                        }
                    });
                });
            });

            initializeSortable();
        }

        // Dynamically add save button if editing is enabled
        if (editing.enabled) {
            const saveButton = document.createElement('button');
            saveButton.textContent = editing.saveButton.title;
            saveButton.className = editing.saveButton.cssClass;
            container.appendChild(saveButton);

            saveButton.addEventListener('click', function () {
                editing.saveButton.events.onSave(this, saveButton);
                editing.saveButton.events.onSaving(this, saveButton);
                sendData.call(this, formData, saveButton, dataSource.endpoints.update);
            }.bind(this));
        }

        /**
         * Generates a UUID.
         * @returns {string} A UUID.
         */
        function generateUUID() {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                const r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            });
        }

        /**
         * Adds a new section to the form.
         */
        function addSection() {
            const sectionId = generateUUID();
            const section = document.createElement('div');
            section.className = 'section';
            section.dataset.id = sectionId;
            section.innerHTML = `
                <button class="collapsible govuk-button govuk-button--secondary">Section</button>
                <button class="deleteButton govuk-button govuk-button--warning" data-type="section" data-id="${sectionId}" aria-label="Delete Section">X</button>
                <div class="content">
                    <button class="govuk-button" onclick="formBuilderInstance.addPage(this, '${sectionId}')">Add Page</button>
                    <div class="pagesContainer"></div>
                </div>
            `;
            document.getElementById('sectionsContainer').appendChild(section);

            formData.sections.push({
                id: sectionId,
                pages: []
            });
            if (options.previewEnabled) updatePreview();
            initializeSortable();
        }

        /**
         * Adds a new page to a section.
         * @param {HTMLElement} sectionElement - The section element to add the page to.
         * @param {string} sectionId - The ID of the section.
         */
        function addPage(sectionElement, sectionId) {
            const pageId = generateUUID();
            const page = document.createElement('div');
            page.className = 'page';
            page.dataset.id = pageId;
            page.innerHTML = `
                <button class="collapsible govuk-button govuk-button--secondary">Page</button>
                <button class="deleteButton govuk-button govuk-button--warning" data-type="page" data-id="${pageId}" data-section-id="${sectionId}" aria-label="Delete Page">X</button>
                <div class="content">
                    <button class="govuk-button" onclick="formBuilderInstance.addQuestion(this, '${sectionId}', '${pageId}')">Add Question</button>
                    <div class="questionsContainer"></div>
                </div>
            `;
            sectionElement.nextElementSibling.appendChild(page);

            const section = formData.sections.find(s => s.id === sectionId);
            section.pages.push({
                id: pageId,
                questions: []
            });
            if (options.previewEnabled) updatePreview();
            initializeSortable();
        }

        /**
         * Adds a new question to a page.
         * @param {HTMLElement} pageElement - The page element to add the question to.
         * @param {string} sectionId - The ID of the section.
         * @param {string} pageId - The ID of the page.
         */
        function addQuestion(pageElement, sectionId, pageId) {
            const questionId = generateUUID();
            const question = document.createElement('div');
            question.className = 'question';
            question.dataset.id = questionId;
            question.innerHTML = `
                <button class="collapsible govuk-button govuk-button--secondary">Question</button>
                <button class="deleteButton govuk-button govuk-button--warning" data-type="question" data-id="${questionId}" data-section-id="${sectionId}" data-page-id="${pageId}" aria-label="Delete Question">X</button>
                <div class="content">
                    <label>Text: <input type="text" name="questionText" oninput="formBuilderInstance.updateQuestionText('${sectionId}', '${pageId}', '${questionId}', this.value)"></label>
                    <label>Description: <input type="text" name="questionDescription" oninput="formBuilderInstance.updateQuestionDescription('${sectionId}', '${pageId}', '${questionId}', this.value)"></label>
                    <label>Hint: <input type="text" name="questionHint" oninput="formBuilderInstance.updateQuestionHint('${sectionId}', '${pageId}', '${questionId}', this.value)"></label>
                    <label>Type:
                        <select name="questionType" onchange="formBuilderInstance.updateQuestionType('${sectionId}', '${pageId}', '${questionId}', this.value, this)">
                            <option value="text">Text</option>
                            <option value="textarea">Text Area</option>
                            <option value="number">Number</option>
                            <option value="date">Date</option>
                            <option value="checkbox">Checkbox</option>
                            <option value="radio">Radio</option>
                        </select>
                    </label>
                    <div class="optionsContainer"></div>
                </div>
            `;
            pageElement.nextElementSibling.appendChild(question);

            const section = formData.sections.find(s => s.id === sectionId);
            const page = section.pages.find(p => p.id === pageId);
            page.questions.push({
                id: questionId,
                text: '',
                description: '',
                hint: '',
                type: 'text',
                options: []
            });
            if (options.previewEnabled) updatePreview();
            initializeSortable();
        }

        /**
         * Deletes a section from the form.
         * @param {string} sectionId - The ID of the section to delete.
         */
        function deleteSection(sectionId) {
            const sectionIndex = formData.sections.findIndex(s => s.id === sectionId);
            formData.sections.splice(sectionIndex, 1);
            document.querySelector(`.section[data-id="${sectionId}"]`).remove();
            if (options.previewEnabled) updatePreview();
            if (options.previewEnabled && options.previewOptions.showJSONSourceTab) viewFormData();
        }

        /**
         * Deletes a page from a section.
         * @param {string} sectionId - The ID of the section.
         * @param {string} pageId - The ID of the page to delete.
         */
        function deletePage(sectionId, pageId) {
            const section = formData.sections.find(s => s.id === sectionId);
            const pageIndex = section.pages.findIndex(p => p.id === pageId);
            section.pages.splice(pageIndex, 1);
            document.querySelector(`.page[data-id="${pageId}"]`).remove();
            if (options.previewEnabled) updatePreview();
            if (options.previewEnabled && options.previewOptions.showJSONSourceTab) viewFormData();
        }

        /**
         * Deletes a question from a page.
         * @param {string} sectionId - The ID of the section.
         * @param {string} pageId - The ID of the page.
         * @param {string} questionId - The ID of the question to delete.
         */
        function deleteQuestion(sectionId, pageId, questionId) {
            const section = formData.sections.find(s => s.id === sectionId);
            const page = section.pages.find(p => p.id === pageId);
            const questionIndex = page.questions.findIndex(q => q.id === questionId);
            page.questions.splice(questionIndex, 1);
            document.querySelector(`.question[data-id="${questionId}"]`).remove();
            if (options.previewEnabled) updatePreview();
            if (options.previewEnabled && options.previewOptions.showJSONSourceTab) viewFormData();
        }

        /**
         * Deletes an option from a question.
         * @param {string} sectionId - The ID of the section.
         * @param {string} pageId - The ID of the page.
         * @param {string} questionId - The ID of the question.
         * @param {string} optionId - The ID of the option to delete.
         */
        function deleteOption(sectionId, pageId, questionId, optionId) {
            const section = formData.sections.find(s => s.id === sectionId);
            const page = section.pages.find(p => p.id === pageId);
            const question = page.questions.find(q => q.id === questionId);
            const optionIndex = question.options.findIndex(o => o.id === optionId);
            question.options.splice(optionIndex, 1);
            document.querySelector(`.option[data-id="${optionId}"]`).remove();
            if (options.previewEnabled) updatePreview();
            if (options.previewEnabled && options.previewOptions.showJSONSourceTab) viewFormData();
        }

        /**
         * Toggles the visibility of collapsible content.
         * @param {HTMLElement} element - The collapsible button element.
         */
        function toggleCollapsible(element) {
            element.classList.toggle('active');
            const content = element.nextElementSibling.nextElementSibling;
            content.classList.toggle('show');
        }

        /**
         * Updates the text of a question.
         * @param {string} sectionId - The ID of the section.
         * @param {string} pageId - The ID of the page.
         * @param {string} questionId - The ID of the question.
         * @param {string} value - The new text value.
         */
        function updateQuestionText(sectionId, pageId, questionId, value) {
            const section = formData.sections.find(s => s.id === sectionId);
            const page = section.pages.find(p => p.id === pageId);
            const question = page.questions.find(q => q.id === questionId);
            question.text = value;
            if (options.previewEnabled) updatePreview();
        }

        /**
         * Updates the description of a question.
         * @param {string} sectionId - The ID of the section.
         * @param {string} pageId - The ID of the page.
         * @param {string} questionId - The ID of the question.
         * @param {string} value - The new description value.
         */
        function updateQuestionDescription(sectionId, pageId, questionId, value) {
            const section = formData.sections.find(s => s.id === sectionId);
            const page = section.pages.find(p => p.id === pageId);
            const question = page.questions.find(q => q.id === questionId);
            question.description = value;
            if (options.previewEnabled) updatePreview();
        }

        /**
         * Updates the hint of a question.
         * @param {string} sectionId - The ID of the section.
         * @param {string} pageId - The ID of the page.
         * @param {string} questionId - The ID of the question.
         * @param {string} value - The new hint value.
         */
        function updateQuestionHint(sectionId, pageId, questionId, value) {
            const section = formData.sections.find(s => s.id === sectionId);
            const page = section.pages.find(p => p.id === pageId);
            const question = page.questions.find(q => q.id === questionId);
            question.hint = value;
            if (options.previewEnabled) updatePreview();
        }

        /**
         * Updates the type of a question and handles options for checkbox and radio types.
         * @param {string} sectionId - The ID of the section.
         * @param {string} pageId - The ID of the page.
         * @param {string} questionId - The ID of the question.
         * @param {string} value - The new type value.
         * @param {HTMLElement} selectElement - The select element that triggered the change.
         */
        function updateQuestionType(sectionId, pageId, questionId, value, selectElement) {
            const section = formData.sections.find(s => s.id === sectionId);
            const page = section.pages.find(p => p.id === pageId);
            const question = page.questions.find(q => q.id === questionId);
            question.type = value;

            const optionsContainer = selectElement.parentElement.nextElementSibling;
            optionsContainer.innerHTML = '';

            if (value === 'checkbox' || value === 'radio') {
                const addOptionBtn = document.createElement('button');
                addOptionBtn.textContent = 'Add Option';
                addOptionBtn.type = 'button';
                addOptionBtn.classList.add('govuk-button');
                addOptionBtn.onclick = () => addOption(sectionId, pageId, questionId, optionsContainer);
                optionsContainer.appendChild(addOptionBtn);
            }

            if (options.previewEnabled) updatePreview();
        }

        /**
         * Adds a new option to a question.
         * @param {string} sectionId - The ID of the section.
         * @param {string} pageId - The ID of the page.
         * @param {string} questionId - The ID of the question.
         * @param {HTMLElement} optionsContainer - The container element for the options.
         */
        function addOption(sectionId, pageId, questionId, optionsContainer) {
            const section = formData.sections.find(s => s.id === sectionId);
            const page = section.pages.find(p => p.id === pageId);
            const question = page.questions.find(q => q.id === questionId);

            const optionId = generateUUID();
            question.options.push({ id: optionId, text: '' });

            const optionElement = document.createElement('div');
            optionElement.className = 'option';
            optionElement.dataset.id = optionId;
            optionElement.innerHTML = `
                <label>Option: <input type="text" oninput="formBuilderInstance.updateOptionText('${sectionId}', '${pageId}', '${questionId}', '${optionId}', this.value)"></label>
                <button class="deleteButton govuk-button govuk-button--warning" data-type="option" data-id="${optionId}" data-section-id="${sectionId}" data-page-id="${pageId}" data-question-id="${questionId}" aria-label="Delete Option">X</button>
            `;
            optionsContainer.appendChild(optionElement);

            if (options.previewEnabled) updatePreview();
            initializeSortable();
        }

        /**
         * Updates the text of an option.
         * @param {string} sectionId - The ID of the section.
         * @param {string} pageId - The ID of the page.
         * @param {string} questionId - The ID of the question.
         * @param {string} optionId - The ID of the option.
         * @param {string} value - The new text value.
         */
        function updateOptionText(sectionId, pageId, questionId, optionId, value) {
            const section = formData.sections.find(s => s.id === sectionId);
            const page = section.pages.find(p => p.id === pageId);
            const question = page.questions.find(q => q.id === questionId);
            const option = question.options.find(o => o.id === optionId);
            option.text = value;
            if (options.previewEnabled) updatePreview();
        }

        /**
         * Displays the form data as JSON.
         */
        function viewFormData() {
            document.getElementById('jsonContainer').innerText = JSON.stringify(formData, null, 2);
        }

        /**
         * Updates the form preview.
         */
        function updatePreview() {
            const previewContainer = document.getElementById('previewContainer');
            previewContainer.innerHTML = '';

            formData.sections.forEach(section => {
                const sectionElement = document.createElement('div');
                sectionElement.className = 'section';
                sectionElement.innerHTML = `<h2>Section</h2>`;

                section.pages.forEach(page => {
                    const pageElement = document.createElement('div');
                    pageElement.className = 'page';
                    pageElement.innerHTML = `<h3>Page</h3>`;

                    page.questions.forEach(question => {
                        const questionElement = document.createElement('div');
                        questionElement.className = 'question';
                        questionElement.innerHTML = `
                            <h4>Question</h4>
                            <p>Text: ${question.text}</p>
                            <p>Description: ${question.description}</p>
                            <p>Hint: ${question.hint}</p>
                            <p>Type: ${question.type}</p>
                        `;

                        if (question.type === 'checkbox' || question.type === 'radio') {
                            question.options.forEach(option => {
                                const optionElement = document.createElement('div');
                                optionElement.innerHTML = `
                                    <label>
                                        <input type="${question.type}" name="question${question.id}" value="${option.text}">
                                        ${option.text}
                                    </label>
                                `;
                                questionElement.appendChild(optionElement);
                            });
                        } else {
                            questionElement.innerHTML += questionTemplates[question.type];
                        }

                        pageElement.appendChild(questionElement);
                    });

                    sectionElement.appendChild(pageElement);
                });

                previewContainer.appendChild(sectionElement);
            });
        }

        /**
         * Initializes sortable functionality for sections, pages, questions, and options.
         */
        function initializeSortable() {
            new Sortable(document.getElementById('sectionsContainer'), {
                animation: 150,
                onEnd: function (evt) {
                    const newOrder = Array.from(document.querySelectorAll('#sectionsContainer .section')).map(section => section.dataset.id);
                    formData.sections = newOrder.map(id => formData.sections.find(section => section.id === id));
                    if (options.previewEnabled) updatePreview();
                    if (options.previewEnabled && options.previewOptions.showJSONSourceTab) viewFormData();
                }
            });

            document.querySelectorAll('.pagesContainer').forEach(container => {
                new Sortable(container, {
                    animation: 150,
                    onEnd: function (evt) {
                        const sectionId = container.closest('.section').dataset.id;
                        const section = formData.sections.find(s => s.id === sectionId);
                        const newOrder = Array.from(container.querySelectorAll('.page')).map(page => page.dataset.id);
                        section.pages = newOrder.map(id => section.pages.find(page => page.id === id));
                        if (options.previewEnabled) updatePreview();
                        if (options.previewEnabled && options.previewOptions.showJSONSourceTab) viewFormData();
                    }
                });
            });

            document.querySelectorAll('.questionsContainer').forEach(container => {
                new Sortable(container, {
                    animation: 150,
                    onEnd: function (evt) {
                        const sectionId = container.closest('.section').dataset.id;
                        const pageId = container.closest('.page').dataset.id;
                        const section = formData.sections.find(s => s.id === sectionId);
                        const page = section.pages.find(p => p.id === pageId);
                        const newOrder = Array.from(container.querySelectorAll('.question')).map(question => question.dataset.id);
                        page.questions = newOrder.map(id => page.questions.find(question => question.id === id));
                        if (options.previewEnabled) updatePreview();
                        if (options.previewEnabled && options.previewOptions.showJSONSourceTab) viewFormData();
                    }
                });
            });

            document.querySelectorAll('.optionsContainer').forEach(container => {
                new Sortable(container, {
                    animation: 150,
                    onEnd: function (evt) {
                        const sectionId = container.closest('.section').dataset.id;
                        const pageId = container.closest('.page').dataset.id;
                        const questionId = container.closest('.question').dataset.id;
                        const section = formData.sections.find(s => s.id === sectionId);
                        const page = section.pages.find(p => p.id === pageId);
                        const question = page.questions.find(q => q.id === questionId);
                        const newOrder = Array.from(container.querySelectorAll('.option')).map(option => option.dataset.id);
                        question.options = newOrder.map(id => question.options.find(option => option.id === id));
                        if (options.previewEnabled) updatePreview();
                        if (options.previewEnabled && options.previewOptions.showJSONSourceTab) viewFormData();
                    }
                });
            });
        }

        /**
         * Shows the specified tab (preview or JSON source).
         * @param {string} tabName - The name of the tab to show.
         */
        function showTab(tabName) {
            const previewContainer = document.getElementById('previewContainer');
            const jsonContainer = document.getElementById('jsonContainer');
            if (tabName === 'preview') {
                previewContainer.style.display = 'block';
                jsonContainer.style.display = 'none';
            } else {
                previewContainer.style.display = 'none';
                jsonContainer.style.display = 'block';
                viewFormData();
            }
        }

        // Expose functions to the formBuilderInstance
        this.addPage = addPage;
        this.addQuestion = addQuestion;
        this.updateQuestionText = updateQuestionText;
        this.updateQuestionDescription = updateQuestionDescription;
        this.updateQuestionHint = updateQuestionHint;
        this.updateQuestionType = updateQuestionType;
        this.addOption = addOption;
        this.updateOptionText = updateOptionText;
        this.showTab = showTab;
        this.loadFormData = this.loadFormData;

        // Trigger the load event if the read endpoint is provided
        if (dataSource.endpoints.read) {
            loadData.call(this);
        }
    }

    // Assign the form builder to the global window object
    window.fujitsuFormBuilder = fujitsuFormBuilder;
})();