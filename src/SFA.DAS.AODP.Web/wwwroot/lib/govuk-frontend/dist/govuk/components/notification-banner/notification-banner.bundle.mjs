function normaliseString(value, property) {
  const trimmedValue = value ? value.trim() : '';
  let output;
  let outputType = property == null ? void 0 : property.type;
  if (!outputType) {
    if (['true', 'false'].includes(trimmedValue)) {
      outputType = 'boolean';
    }
    if (trimmedValue.length > 0 && isFinite(Number(trimmedValue))) {
      outputType = 'number';
    }
  }
  switch (outputType) {
    case 'boolean':
      output = trimmedValue === 'true';
      break;
    case 'number':
      output = Number(trimmedValue);
      break;
    default:
      output = value;
  }
  return output;
}

/**
 * @typedef {import('./index.mjs').SchemaProperty} SchemaProperty
 */

function mergeConfigs(...configObjects) {
  const formattedConfigObject = {};
  for (const configObject of configObjects) {
    for (const key of Object.keys(configObject)) {
      const option = formattedConfigObject[key];
      const override = configObject[key];
      if (isObject(option) && isObject(override)) {
        formattedConfigObject[key] = mergeConfigs(option, override);
      } else {
        formattedConfigObject[key] = override;
      }
    }
  }
  return formattedConfigObject;
}
function extractConfigByNamespace(Component, dataset, namespace) {
  const property = Component.schema.properties[namespace];
  if ((property == null ? void 0 : property.type) !== 'object') {
    return;
  }
  const newObject = {
    [namespace]: ({})
  };
  for (const [key, value] of Object.entries(dataset)) {
    let current = newObject;
    const keyParts = key.split('.');
    for (const [index, name] of keyParts.entries()) {
      if (typeof current === 'object') {
        if (index < keyParts.length - 1) {
          if (!isObject(current[name])) {
            current[name] = {};
          }
          current = current[name];
        } else if (key !== namespace) {
          current[name] = normaliseString(value);
        }
      }
    }
  }
  return newObject[namespace];
}
function setFocus($element, options = {}) {
  var _options$onBeforeFocu;
  const isFocusable = $element.getAttribute('tabindex');
  if (!isFocusable) {
    $element.setAttribute('tabindex', '-1');
  }
  function onFocus() {
    $element.addEventListener('blur', onBlur, {
      once: true
    });
  }
  function onBlur() {
    var _options$onBlur;
    (_options$onBlur = options.onBlur) == null || _options$onBlur.call($element);
    if (!isFocusable) {
      $element.removeAttribute('tabindex');
    }
  }
  $element.addEventListener('focus', onFocus, {
    once: true
  });
  (_options$onBeforeFocu = options.onBeforeFocus) == null || _options$onBeforeFocu.call($element);
  $element.focus();
}
function isInitialised($root, moduleName) {
  return $root instanceof HTMLElement && $root.hasAttribute(`data-${moduleName}-init`);
}

/**
 * Checks if GOV.UK Frontend is supported on this page
 *
 * Some browsers will load and run our JavaScript but GOV.UK Frontend
 * won't be supported.
 *
 * @param {HTMLElement | null} [$scope] - (internal) `<body>` HTML element checked for browser support
 * @returns {boolean} Whether GOV.UK Frontend is supported on this page
 */
function isSupported($scope = document.body) {
  if (!$scope) {
    return false;
  }
  return $scope.classList.contains('govuk-frontend-supported');
}
function isArray(option) {
  return Array.isArray(option);
}
function isObject(option) {
  return !!option && typeof option === 'object' && !isArray(option);
}
function formatErrorMessage(Component, message) {
  return `${Component.moduleName}: ${message}`;
}

/**
 * Schema for component config
 *
 * @typedef {object} Schema
 * @property {{ [field: string]: SchemaProperty | undefined }} properties - Schema properties
 * @property {SchemaCondition[]} [anyOf] - List of schema conditions
 */

/**
 * Schema property for component config
 *
 * @typedef {object} SchemaProperty
 * @property {'string' | 'boolean' | 'number' | 'object'} type - Property type
 */

/**
 * Schema condition for component config
 *
 * @typedef {object} SchemaCondition
 * @property {string[]} required - List of required config fields
 * @property {string} errorMessage - Error message when required config fields not provided
 */
/**
 * @typedef ComponentWithModuleName
 * @property {string} moduleName - Name of the component
 */

function normaliseDataset(Component, dataset) {
  const out = {};
  for (const [field, property] of Object.entries(Component.schema.properties)) {
    if (field in dataset) {
      out[field] = normaliseString(dataset[field], property);
    }
    if ((property == null ? void 0 : property.type) === 'object') {
      out[field] = extractConfigByNamespace(Component, dataset, field);
    }
  }
  return out;
}

class GOVUKFrontendError extends Error {
  constructor(...args) {
    super(...args);
    this.name = 'GOVUKFrontendError';
  }
}
class SupportError extends GOVUKFrontendError {
  /**
   * Checks if GOV.UK Frontend is supported on this page
   *
   * @param {HTMLElement | null} [$scope] - HTML element `<body>` checked for browser support
   */
  constructor($scope = document.body) {
    const supportMessage = 'noModule' in HTMLScriptElement.prototype ? 'GOV.UK Frontend initialised without `<body class="govuk-frontend-supported">` from template `<script>` snippet' : 'GOV.UK Frontend is not supported in this browser';
    super($scope ? supportMessage : 'GOV.UK Frontend initialised without `<script type="module">`');
    this.name = 'SupportError';
  }
}
class ElementError extends GOVUKFrontendError {
  constructor(messageOrOptions) {
    let message = typeof messageOrOptions === 'string' ? messageOrOptions : '';
    if (typeof messageOrOptions === 'object') {
      const {
        component,
        identifier,
        element,
        expectedType
      } = messageOrOptions;
      message = identifier;
      message += element ? ` is not of type ${expectedType != null ? expectedType : 'HTMLElement'}` : ' not found';
      message = formatErrorMessage(component, message);
    }
    super(message);
    this.name = 'ElementError';
  }
}
class InitError extends GOVUKFrontendError {
  constructor(componentOrMessage) {
    const message = typeof componentOrMessage === 'string' ? componentOrMessage : formatErrorMessage(componentOrMessage, `Root element (\`$root\`) already initialised`);
    super(message);
    this.name = 'InitError';
  }
}
/**
 * @typedef {import('../common/index.mjs').ComponentWithModuleName} ComponentWithModuleName
 */

class GOVUKFrontendComponent {
  /**
   * Returns the root element of the component
   *
   * @protected
   * @returns {RootElementType} - the root element of component
   */
  get $root() {
    return this._$root;
  }
  constructor($root) {
    this._$root = void 0;
    const childConstructor = this.constructor;
    if (typeof childConstructor.moduleName !== 'string') {
      throw new InitError(`\`moduleName\` not defined in component`);
    }
    if (!($root instanceof childConstructor.elementType)) {
      throw new ElementError({
        element: $root,
        component: childConstructor,
        identifier: 'Root element (`$root`)',
        expectedType: childConstructor.elementType.name
      });
    } else {
      this._$root = $root;
    }
    childConstructor.checkSupport();
    this.checkInitialised();
    const moduleName = childConstructor.moduleName;
    this.$root.setAttribute(`data-${moduleName}-init`, '');
  }
  checkInitialised() {
    const constructor = this.constructor;
    const moduleName = constructor.moduleName;
    if (moduleName && isInitialised(this.$root, moduleName)) {
      throw new InitError(constructor);
    }
  }
  static checkSupport() {
    if (!isSupported()) {
      throw new SupportError();
    }
  }
}

/**
 * @typedef ChildClass
 * @property {string} moduleName - The module name that'll be looked for in the DOM when initialising the component
 */

/**
 * @typedef {typeof GOVUKFrontendComponent & ChildClass} ChildClassConstructor
 */
GOVUKFrontendComponent.elementType = HTMLElement;

/**
 * Notification Banner component
 *
 * @preserve
 */
class NotificationBanner extends GOVUKFrontendComponent {
  /**
   * @param {Element | null} $root - HTML element to use for notification banner
   * @param {NotificationBannerConfig} [config] - Notification banner config
   */
  constructor($root, config = {}) {
    super($root);
    this.config = void 0;
    this.config = mergeConfigs(NotificationBanner.defaults, config, normaliseDataset(NotificationBanner, this.$root.dataset));
    if (this.$root.getAttribute('role') === 'alert' && !this.config.disableAutoFocus) {
      setFocus(this.$root);
    }
  }
}

/**
 * Notification banner config
 *
 * @typedef {object} NotificationBannerConfig
 * @property {boolean} [disableAutoFocus=false] - If set to `true` the
 *   notification banner will not be focussed when the page loads. This only
 *   applies if the component has a `role` of `alert` – in other cases the
 *   component will not be focused on page load, regardless of this option.
 */

/**
 * @typedef {import('../../common/index.mjs').Schema} Schema
 */
NotificationBanner.moduleName = 'govuk-notification-banner';
NotificationBanner.defaults = Object.freeze({
  disableAutoFocus: false
});
NotificationBanner.schema = Object.freeze({
  properties: {
    disableAutoFocus: {
      type: 'boolean'
    }
  }
});

export { NotificationBanner };
//# sourceMappingURL=notification-banner.bundle.mjs.map
