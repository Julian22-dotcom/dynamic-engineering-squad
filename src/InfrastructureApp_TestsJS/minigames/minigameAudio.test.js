const { loadScript } = require("./testUtils");

describe("minigameAudio.js", () => {
    let audioInstances;
    let warnSpy;

    class MockAudio {
        constructor(src) {
            this.src = src;
            this.loop = false;
            this.preload = "";
            this.muted = false;
            this.currentTime = 4;
            this.pause = jest.fn();
            this.play = jest.fn(() => Promise.resolve());
            this.listeners = {};
            audioInstances.push(this);
        }

        addEventListener(type, callback) {
            this.listeners[type] = this.listeners[type] || [];
            this.listeners[type].push(callback);
        }

        dispatch(type) {
            (this.listeners[type] || []).forEach((listener) => listener());
        }
    }

    beforeEach(() => {
        jest.resetModules();
        document.body.innerHTML = "";
        audioInstances = [];
        global.Audio = MockAudio;
        warnSpy = jest.spyOn(console, "warn").mockImplementation(() => { });
        loadScript("minigameAudio.js");
    });

    afterEach(() => {
        jest.restoreAllMocks();
        delete global.Audio;
    });

    test("defines window.createMinigameAudio", () => {
        expect(typeof window.createMinigameAudio).toBe("function");
    });

    test("returns noop controller when no src is provided", () => {
        const controller = window.createMinigameAudio();

        expect(controller).toBeTruthy();
        expect(() => controller.play()).not.toThrow();
        expect(() => controller.stop()).not.toThrow();
        expect(controller.toggleMute()).toBe(true);
        expect(controller.toggleMute()).toBe(false);
        expect(warnSpy).toHaveBeenCalled();
    });

    test("creates Audio with defaults when src is provided", () => {
        const controller = window.createMinigameAudio({ src: "/audio/minigames/theme.mp3" });

        expect(controller).toBeTruthy();
        expect(audioInstances).toHaveLength(1);
        expect(audioInstances[0].src).toBe("/audio/minigames/theme.mp3");
        expect(audioInstances[0].loop).toBe(true);
        expect(audioInstances[0].preload).toBe("none");
    });

    test("respects loop false when provided", () => {
        window.createMinigameAudio({ src: "/audio/minigames/theme.mp3", loop: false });

        expect(audioInstances[0].loop).toBe(false);
    });

    test("play calls audio.play and stop pauses and resets currentTime", async () => {
        const controller = window.createMinigameAudio({ src: "/audio/minigames/theme.mp3" });

        controller.play();
        expect(audioInstances[0].play).toHaveBeenCalledTimes(1);

        controller.stop();
        expect(audioInstances[0].pause).toHaveBeenCalledTimes(1);
        expect(audioInstances[0].currentTime).toBe(0);
    });

    test("toggleMute updates audio muted state and returns the new state", () => {
        const controller = window.createMinigameAudio({ src: "/audio/minigames/theme.mp3" });

        expect(controller.toggleMute()).toBe(true);
        expect(audioInstances[0].muted).toBe(true);
        expect(controller.toggleMute()).toBe(false);
        expect(audioInstances[0].muted).toBe(false);
    });

    test("audio error event prevents later play attempts", () => {
        const controller = window.createMinigameAudio({ src: "/audio/minigames/theme.mp3" });
        audioInstances[0].dispatch("error");

        controller.play();

        expect(audioInstances[0].play).not.toHaveBeenCalled();
        expect(warnSpy).toHaveBeenCalled();
    });

    test("beforeunload and pagehide stop audio", () => {
        window.createMinigameAudio({ src: "/audio/minigames/theme.mp3" });

        window.dispatchEvent(new Event("beforeunload"));
        expect(audioInstances[0].pause).toHaveBeenCalledTimes(1);
        expect(audioInstances[0].currentTime).toBe(0);

        audioInstances[0].currentTime = 3;
        window.dispatchEvent(new Event("pagehide"));
        expect(audioInstances[0].pause).toHaveBeenCalledTimes(2);
        expect(audioInstances[0].currentTime).toBe(0);
    });

    test("visibilitychange hidden pauses audio", () => {
        window.createMinigameAudio({ src: "/audio/minigames/theme.mp3" });

        Object.defineProperty(document, "visibilityState", {
            configurable: true,
            value: "hidden"
        });

        document.dispatchEvent(new Event("visibilitychange"));

        expect(audioInstances[0].pause).toHaveBeenCalledTimes(1);
    });
});
