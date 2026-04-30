(function (window) {
    function createMinigameAudio(options) {
        const settings = options || {};
        const src = settings.src;
        const loop = settings.loop !== false;

        if (!src) {
            return createNoopController();
        }

        const audio = new Audio(src);
        audio.loop = loop;
        audio.preload = "none";

        let muted = false;
        let failed = false;

        audio.addEventListener("error", function () {
            failed = true;
        });

        function play() {
            if (failed) {
                return;
            }

            audio.muted = muted;
            const promise = audio.play();
            if (promise && typeof promise.catch === "function") {
                promise.catch(function () {
                    failed = true;
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

    window.createMinigameAudio = createMinigameAudio;
})(window);
