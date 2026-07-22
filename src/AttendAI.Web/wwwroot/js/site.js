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

    document.querySelectorAll('[data-copy-target]').forEach(function (button) {
        button.addEventListener('click', function () {
            var target = document.querySelector(button.getAttribute('data-copy-target'));
            if (!target || !navigator.clipboard) {
                return;
            }

            navigator.clipboard.writeText(target.textContent.trim()).then(function () {
                button.dataset.copied = 'true';
                setTimeout(function () {
                    delete button.dataset.copied;
                }, 1500);
            });
        });
    });

    function updateCountdowns() {
        document.querySelectorAll('[data-countdown-target]').forEach(function (element) {
            var targetValue = element.getAttribute('data-countdown-target');
            if (!targetValue) {
                element.textContent = '--:--';
                return;
            }

            var remaining = new Date(targetValue).getTime() - Date.now();
            if (!Number.isFinite(remaining) || remaining <= 0) {
                element.textContent = '00:00';
                return;
            }

            var totalSeconds = Math.floor(remaining / 1000);
            var hours = Math.floor(totalSeconds / 3600);
            var minutes = Math.floor((totalSeconds % 3600) / 60);
            var seconds = totalSeconds % 60;
            element.textContent = hours > 0
                ? String(hours).padStart(2, '0') + ':' + String(minutes).padStart(2, '0') + ':' + String(seconds).padStart(2, '0')
                : String(minutes).padStart(2, '0') + ':' + String(seconds).padStart(2, '0');
        });
    }

    if (document.querySelector('[data-countdown-target]')) {
        updateCountdowns();
        setInterval(updateCountdowns, 1000);
    }

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') {
            document.body.classList.remove('sidebar-open');
        }
    });
})();
