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

    document.querySelectorAll('[data-biometric-camera]').forEach(function (root) {
        var required = Number(root.getAttribute('data-required-captures')) || 3;
        var maxBytes = Number(root.getAttribute('data-max-capture-bytes')) || 1500000;
        var video = root.querySelector('[data-biometric-video]');
        var canvas = root.querySelector('[data-biometric-canvas]');
        var placeholder = root.querySelector('[data-biometric-placeholder]');
        var fileInput = root.querySelector('[data-biometric-file-input]');
        var startButton = root.querySelector('[data-biometric-start]');
        var captureButton = root.querySelector('[data-biometric-capture]');
        var clearButton = root.querySelector('[data-biometric-clear]');
        var submitButton = root.querySelector('[data-biometric-submit]');
        var strip = root.querySelector('[data-biometric-strip]');
        var count = root.querySelector('[data-biometric-count]');
        var stream;
        var captures = [];

        function stopCamera() {
            if (stream) {
                stream.getTracks().forEach(function (track) {
                    track.stop();
                });
                stream = null;
            }
        }

        function syncFiles() {
            if (!fileInput || typeof DataTransfer === 'undefined') {
                return;
            }

            var transfer = new DataTransfer();
            captures.forEach(function (capture) {
                transfer.items.add(capture);
            });
            fileInput.files = transfer.files;
        }

        function renderCaptures() {
            if (count) {
                count.textContent = String(captures.length);
            }

            if (strip) {
                strip.innerHTML = '';
                captures.forEach(function (file) {
                    var holder = document.createElement('div');
                    var image = document.createElement('img');
                    holder.className = 'capture-thumb';
                    image.alt = '';
                    image.src = URL.createObjectURL(file);
                    image.addEventListener('load', function () {
                        URL.revokeObjectURL(image.src);
                    }, { once: true });
                    holder.appendChild(image);
                    strip.appendChild(holder);
                });
            }

            if (clearButton) {
                clearButton.disabled = captures.length === 0;
            }

            if (submitButton) {
                submitButton.disabled = captures.length < required;
            }

            syncFiles();
        }

        function addCapture(blob) {
            if (!blob || blob.size > maxBytes || captures.length >= required) {
                return;
            }

            captures.push(new File([blob], 'capture-' + captures.length + '.jpg', { type: 'image/jpeg' }));
            renderCaptures();
        }

        if (startButton && video && navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
            startButton.addEventListener('click', function () {
                navigator.mediaDevices.getUserMedia({ video: { facingMode: 'user' }, audio: false }).then(function (mediaStream) {
                    stream = mediaStream;
                    video.srcObject = stream;
                    video.play();
                    if (placeholder) {
                        placeholder.hidden = true;
                    }
                    if (captureButton) {
                        captureButton.disabled = false;
                    }
                }).catch(function () {
                    if (placeholder) {
                        placeholder.textContent = placeholder.textContent;
                    }
                });
            });
        }

        if (captureButton && video && canvas) {
            captureButton.addEventListener('click', function () {
                if (!video.videoWidth || !video.videoHeight) {
                    return;
                }

                canvas.width = video.videoWidth;
                canvas.height = video.videoHeight;
                canvas.getContext('2d').drawImage(video, 0, 0, canvas.width, canvas.height);
                canvas.toBlob(addCapture, 'image/jpeg', 0.9);
            });
        }

        if (clearButton) {
            clearButton.addEventListener('click', function () {
                captures = [];
                renderCaptures();
            });
        }

        if (fileInput) {
            fileInput.addEventListener('change', function () {
                captures = Array.prototype.slice.call(fileInput.files || []).slice(0, required);
                renderCaptures();
            });
        }

        window.addEventListener('pagehide', stopCamera);
        window.addEventListener('beforeunload', stopCamera);
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') {
            document.body.classList.remove('sidebar-open');
        }
    });
})();
