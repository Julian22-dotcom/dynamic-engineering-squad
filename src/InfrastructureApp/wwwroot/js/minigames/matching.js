(function () {
    const board = document.getElementById("matchingBoard");
    const resultElement = document.getElementById("matchingResult");
    const restartButton = document.getElementById("matchingRestartButton");
    const muteButton = document.getElementById("matchingMuteButton");
    const currentPointsElement = document.getElementById("matchingCurrentPoints");
    const dailyProgressElement = document.getElementById("matchingDailyProgress");

    if (!board || !resultElement || !restartButton || !currentPointsElement || !dailyProgressElement) {
        return;
    }

    const symbols = [
        { key: "pothole", label: "Pothole" },
        { key: "cone", label: "Cone" },
        { key: "bridge", label: "Bridge" },
        { key: "traffic-light", label: "Traffic Light" },
        { key: "road-sign", label: "Road Sign" },
        { key: "crosswalk", label: "Crosswalk" }
    ];

    const audioController = window.createMinigameAudio
        ? window.createMinigameAudio({ src: "/audio/minigames/matching-theme.mp3", label: "matching-theme" })
        : null;

    let cards = [];
    let flippedCards = [];
    let matchedPairs = 0;
    let lockBoard = false;
    let completionSubmitted = false;

    restartButton.addEventListener("click", initializeBoard);

    if (muteButton && audioController) {
        muteButton.addEventListener("click", function () {
            const muted = audioController.toggleMute();
            muteButton.textContent = muted ? "Unmute Music" : "Mute Music";
        });
    }

    initializeBoard();

    function initializeBoard() {
        matchedPairs = 0;
        flippedCards = [];
        lockBoard = false;
        completionSubmitted = false;

        if (audioController) {
            audioController.stop();
        }

        cards = shuffle(
            symbols.flatMap(function (symbol) {
                return [
                    { id: `${symbol.key}-a`, key: symbol.key, label: symbol.label },
                    { id: `${symbol.key}-b`, key: symbol.key, label: symbol.label }
                ];
            })
        );

        board.innerHTML = "";
        cards.forEach(function (card) {
            board.appendChild(createCardElement(card));
        });

        resultElement.className = "alert alert-secondary mb-3";
        resultElement.textContent = "Flip cards and find all matching pairs. Each cleared board earns 1 point.";
    }

    function createCardElement(card) {
        const button = document.createElement("button");
        button.type = "button";
        button.className = "matching-card";
        button.dataset.cardId = card.id;
        button.dataset.cardKey = card.key;
        button.setAttribute("aria-label", "Hidden matching card");

        const front = document.createElement("span");
        front.className = "matching-card-face matching-card-face-front";
        front.textContent = "?";

        const back = document.createElement("span");
        back.className = "matching-card-face matching-card-face-back";
        back.textContent = card.label;

        button.appendChild(front);
        button.appendChild(back);
        button.addEventListener("click", onCardClicked);
        return button;
    }

    function onCardClicked(event) {
        if (lockBoard) {
            return;
        }

        const cardElement = event.currentTarget;
        if (cardElement.classList.contains("is-flipped") || cardElement.classList.contains("is-matched")) {
            return;
        }

        if (audioController) {
            audioController.play();
        }

        cardElement.classList.add("is-flipped");
        flippedCards.push(cardElement);

        if (flippedCards.length < 2) {
            return;
        }

        lockBoard = true;

        const [first, second] = flippedCards;
        if (first.dataset.cardKey === second.dataset.cardKey) {
            first.classList.add("is-matched");
            second.classList.add("is-matched");
            flippedCards = [];
            lockBoard = false;
            matchedPairs += 1;

            if (matchedPairs === symbols.length) {
                completeGame();
            }

            return;
        }

        window.setTimeout(function () {
            first.classList.remove("is-flipped");
            second.classList.remove("is-flipped");
            flippedCards = [];
            lockBoard = false;
        }, 700);
    }

    async function completeGame() {
        if (completionSubmitted) {
            return;
        }

        completionSubmitted = true;

        try {
            const response = await fetch(board.dataset.completeUrl, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": getAntiForgeryToken()
                },
                body: JSON.stringify({
                    gameKey: board.dataset.gameKey
                })
            });

            if (!response.ok) {
                if (response.status === 401) {
                    window.location.href = "/Account/Login";
                    return;
                }

                throw new Error("Completion request failed.");
            }

            const data = await response.json();
            currentPointsElement.textContent = data.currentPoints;
            dailyProgressElement.textContent = `${data.dailyPointsEarned} / ${data.dailyPointsLimit}`;

            if (data.awardedPoints > 0) {
                resultElement.className = "alert alert-success mb-3";
                resultElement.textContent = `Board cleared. You earned ${data.awardedPoints} point. Shuffle to play again.`;
            } else {
                resultElement.className = "alert alert-warning mb-3";
                resultElement.textContent = "Board cleared. Today's 5-point matching limit has already been reached.";
            }
        } catch (error) {
            console.error(error);
            completionSubmitted = false;
            resultElement.className = "alert alert-danger mb-3";
            resultElement.textContent = "The matching game completed, but the reward could not be verified right now.";
        } finally {
            if (audioController) {
                audioController.stop();
            }
        }
    }

    function getAntiForgeryToken() {
        const field = document.querySelector('input[name="__RequestVerificationToken"]');
        return field ? field.value : "";
    }

    function shuffle(items) {
        const clone = items.slice();

        for (let index = clone.length - 1; index > 0; index -= 1) {
            const swapIndex = Math.floor(Math.random() * (index + 1));
            const temp = clone[index];
            clone[index] = clone[swapIndex];
            clone[swapIndex] = temp;
        }

        return clone;
    }
})();
