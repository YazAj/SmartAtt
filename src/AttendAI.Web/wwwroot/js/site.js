(function () {
    var themeButton = document.querySelector('[data-theme-toggle]');
    var sidebarToggle = document.querySelector('[data-sidebar-toggle]');
    var sidebarClose = document.querySelector('[data-sidebar-close]');
    var preferences = ['system', 'light', 'dark'];

    function applyTheme(preference) {
        var resolvedPreference = preferences.indexOf(preference) >= 0 ? preference : 'system';
        var isDark = resolvedPreference === 'dark' ||
            (resolvedPreference === 'system' && window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches);

        document.documentElement.dataset.theme = isDark ? 'dark' : 'light';
        document.documentElement.dataset.themePreference = resolvedPreference;
        localStorage.setItem('attendai-theme', resolvedPreference);
    }

    if (themeButton) {
        themeButton.addEventListener('click', function () {
            var current = document.documentElement.dataset.themePreference || 'system';
            var next = preferences[(preferences.indexOf(current) + 1) % preferences.length];
            applyTheme(next);
        });
    }

    if (window.matchMedia) {
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function () {
            if ((document.documentElement.dataset.themePreference || 'system') === 'system') {
                applyTheme('system');
            }
        });
    }

    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function () {
            document.body.classList.toggle('sidebar-open');
        });
    }

    if (sidebarClose) {
        sidebarClose.addEventListener('click', function () {
            document.body.classList.remove('sidebar-open');
        });
    }

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') {
            document.body.classList.remove('sidebar-open');
        }
    });
})();
