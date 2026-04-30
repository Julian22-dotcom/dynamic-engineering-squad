(function () {
    const spinButton = document.getElementById("slotsSpinButton");
    const muteButton = document.getElementById("slotsMuteButton");
    const resultElement = document.getElementById("slotsResult");
    const currentPointsElement = document.getElementById("slotsCurrentPoints");
    const dailyProgressElement = document.getElementById("slotsDailyProgress");
    const reelElements = Array.from(document.querySelectorAll("[data-slot-reel]"));

    if (!spinButton || !resultElement || !currentPointsElement || !dailyProgressElement || reelElements.length !== 3) {
        return;
    }

    const audioController = window.createMinigameAudio
        ? window.createMinigameAudio({ src: "/audio/minigames/slots-theme.mp3" })
        : null;

    spinButton.addEventListener("click", async function () {
        spinButton.disabled = true;

        if (audioController) {
            audioController.play();
        }

        try {
            const response = await fetch(spinButton.dataset.spinUrl, {
                method: "POST",
                headers: {
                    "RequestVerificationToken": getAntiForgeryToken()
                }
            });

            if (!response.ok) {
                if (response.status === 401) {
                    window.location.href = "/Account/Login";
                    return;
                }

                throw new Error("Spin request failed.");
            }

            const data = await response.json();
            renderSymbols(data.symbols || []);
            renderResult(data);
            currentPointsElement.textContent = data.currentPoints;
            dailyProgressElement.textContent = `${data.dailyPointsEarned} / ${data.dailyPointsLimit}`;

            if (!data.hasReachedDailyLimit) {
                spinButton.disabled = false;
            }
        } catch (error) {
            console.error(error);
            resultElement.className = "alert alert-danger mb-3";
            resultElement.textContent = "The slot spin could not be completed right now.";
            spinButton.disabled = false;
        } finally {
            if (audioController) {
                audioController.stop();
            }
        }
    });

    if (muteButton && audioController) {
        muteButton.addEventListener("click", function () {
            const muted = audioController.toggleMute();
            muteButton.textContent = muted ? "Unmute Music" : "Mute Music";
        });
    }

    function renderSymbols(symbols) {
        reelElements.forEach(function (reel, index) {
            const symbol = symbols[index] || "?";
            reel.textContent = toDisplaySymbol(symbol);
        });
    }

    function renderResult(data) {
        if (data.hasReachedDailyLimit && data.awardedPoints === 0) {
            resultElement.className = "alert alert-warning mb-3";
            resultElement.textContent = "You already reached today's 5-point Slots limit.";
            return;
        }

        if (data.isWinningSpin) {
            resultElement.className = "alert alert-success mb-3";
            resultElement.textContent = `${data.resultLabel}. You earned ${data.awardedPoints} point.`;
            return;
        }

        resultElement.className = "alert alert-secondary mb-3";
        resultElement.textContent = "No match this spin. Try again.";
    }

    function toDisplaySymbol(symbol) {
        switch (symbol) {
            case "pothole":
                return "Pothole";
            case "cone":
                return "Cone";
            case "road-sign":
                return "Road Sign";
            case "traffic-light":
                return "Traffic Light";
            case "bridge":
                return "Bridge";
            default:
                return symbol;
        }
    }

    function getAntiForgeryToken() {
        const field = document.querySelector('input[name="__RequestVerificationToken"]');
        return field ? field.value : "";
    }
})();
