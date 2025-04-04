$(document).ready(function () {
    // Show banner if no cookie preference is set
    if (window.location.pathname.includes("/cookie-settings")) {
        $("#cookie-banner-acception").hide();
        $("#cookie-banner").hide();
        let savedCookies = getCookie("cookies_policy");

        if (savedCookies) {
            $("input[name='analytics'][value='" + (savedCookies.usage ? "yes" : "no") + "']").prop("checked", true);
            $("input[name='marketing'][value='" + (savedCookies.marketing ? "yes" : "no") + "']").prop("checked", true);
            $(".success-container").show();
        }
    } else {
        if (!getCookie("cookies_preferences_set")) {
            $("#cookie-banner").show();
            $("#cookie-banner-acception").hide();
        }
    }


    // Accept all cookies
    $("#accept-all").click(function () {
        let cookiePolicy = {
            essential: true,
            usage: true,
            marketing: true,
            version: 1
        };
        setCookie("cookies_policy", JSON.stringify(cookiePolicy), 365);
        setCookie("cookies_preferences_set", "true", 365);
        $("#cookie-banner").hide();
        $("#cookie-banner-acception").show();
    });

    $("#hide-cookie-message").click(function () {
        $("#cookie-banner").hide();
        $("#cookie-banner-acception").hide();
    });

    $(".set-cookie-preferences").click(function () {
        window.location.href = "/cookie-settings";
    });
    $(".btn-save").click(function (event) {
        event.preventDefault(); // Prevent form submission

        let analytics = $("input[name='analytics']:checked").val() === "yes";
        let marketing = $("input[name='marketing']:checked").val() === "yes";

        let cookiePolicy = {
            essential: true,
            usage: analytics,
            marketing: marketing,
            version: 1
        };

        // Save cookie locally
        setCookie("cookies_policy", JSON.stringify(cookiePolicy), 365);
        setCookie("cookies_preferences_set", "true", 365);
        window.location.href = "/cookie-settings";
    });
});

// Function to get cookie value and decode JSON if applicable
function getCookie(name) {
    let match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
    if (match) {
        let value = decodeURIComponent(match[2]);
        try {
            return JSON.parse(value); // Parse JSON if applicable
        } catch (e) {
            return value; // Return as string if not JSON
        }
    }
    return null;
}

// Function to set cookie with proper encoding
function setCookie(name, value, days) {
    let expires = new Date();
    expires.setTime(expires.getTime() + (days * 24 * 60 * 60 * 1000));
    document.cookie = name + "=" + encodeURIComponent(value) + "; expires=" + expires.toUTCString() + "; path=/";
}
