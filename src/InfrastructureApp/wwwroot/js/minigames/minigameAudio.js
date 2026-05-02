(function (window) {
    function createMinigameAudio(options) {
        const settings = options || {};
        const src = settings.src;
        const loop = settings.loop !== false;
        const label = settings.label || src || "minigame-audio";

        if (!src) {
            warn(`${label}: no src was provided. Audio will stay disabled.`);
            return createNoopController();
        }

        const audio = new Audio(src);
        audio.loop = loop;
        audio.preload = "none";

        let muted = false;
        let failed = false;

        audio.addEventListener("error", function () {
            failed = true;
            warn(`${label}: failed to load audio from "${src}". Verify the file exists under wwwroot/audio/minigames and the URL is correct.`);
        });

        function play() {
            if (failed) {
                warn(`${label}: play() skipped because audio is already in a failed state.`);
                return;
            }

            audio.muted = muted;
            const promise = audio.play();
            if (promise && typeof promise.catch === "function") {
                promise.catch(function (error) {
                    warn(`${label}: play() was rejected by the browser. This usually means autoplay/user-interaction rules blocked it, or the file could not be loaded.`, error);
                });
            }
        }

        function stop() {
            audio.pause();
            audio.currentTime = 0;
        }

        function toggleMute() {
            muted = !muted;
            audio.muted = muted;
            return muted;
        }

        window.addEventListener("beforeunload", stop);
        window.addEventListener("pagehide", stop);
        document.addEventListener("visibilitychange", function () {
            if (document.visibilityState === "hidden") {
                audio.pause();
            }
        });

        return {
            play: play,
            stop: stop,
            toggleMute: toggleMute,
            isMuted: function () { return muted; }
        };
    }

    function createNoopController() {
        let muted = false;

        return {
            play: function () { },
            stop: function () { },
            toggleMute: function () {
                muted = !muted;
                return muted;
            },
            isMuted: function () { return muted; }
        };
    }

    function warn(message, error) {
        if (window.console && typeof window.console.warn === "function") {
            if (error) {
                window.console.warn("[minigameAudio] " + message, error);
            } else {
                window.console.warn("[minigameAudio] " + message);
            }
        }
    }

    window.createMinigameAudio = createMinigameAudio;
})(window);
