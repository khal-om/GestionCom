window.fednadShowroom = (() => {
    let frameId = 0;
    let track = null;
    let lastTime = 0;
    let direction = 1;
    let userPaused = false;
    let hoverPaused = false;
    let focusPaused = false;
    let reducedMotion = false;
    let resumeTimer = 0;
    const speed = 25;

    const isPaused = () => userPaused || hoverPaused || focusPaused || reducedMotion;

    const stopFrame = () => {
        if (frameId) cancelAnimationFrame(frameId);
        frameId = 0;
        lastTime = 0;
    };

    const tick = (time) => {
        if (!track || !track.isConnected) {
            stopFrame();
            return;
        }

        if (!isPaused() && track.scrollWidth > track.clientWidth + 2) {
            if (lastTime) {
                const max = track.scrollWidth - track.clientWidth;
                track.scrollLeft += direction * speed * Math.min((time - lastTime) / 1000, 0.05);
                if (track.scrollLeft >= max - 1) {
                    track.scrollLeft = max;
                    direction = -1;
                } else if (track.scrollLeft <= 1) {
                    track.scrollLeft = 0;
                    direction = 1;
                }
            }
            lastTime = time;
        } else {
            lastTime = 0;
        }

        frameId = requestAnimationFrame(tick);
    };

    const start = () => {
        if (!frameId && track) frameId = requestAnimationFrame(tick);
    };

    const pauseForManualControl = () => {
        if (resumeTimer) clearTimeout(resumeTimer);
        hoverPaused = true;
        resumeTimer = setTimeout(() => {
            hoverPaused = false;
            start();
        }, 2200);
    };

    const scrollProducts = (element, amount) => {
        if (!element) return;
        pauseForManualControl();
        const card = element.querySelector(".fn-product-card");
        const styles = window.getComputedStyle(element);
        const gap = parseFloat(styles.columnGap || styles.gap || "0");
        const distance = card ? card.getBoundingClientRect().width + gap : element.clientWidth * 0.8;
        element.scrollBy({
            left: amount * distance,
            behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches ? "auto" : "smooth"
        });
        direction = amount > 0 ? 1 : -1;
    };

    const initProductAutoplay = (element, initiallyPaused) => {
        if (!element) return;
        if (track && track !== element) stopFrame();
        track = element;
        userPaused = !!initiallyPaused;
        reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
        element.addEventListener("pointerenter", () => { hoverPaused = true; });
        element.addEventListener("pointerleave", () => { hoverPaused = false; start(); });
        element.addEventListener("focusin", () => { focusPaused = true; });
        element.addEventListener("focusout", (event) => {
            if (!element.contains(event.relatedTarget)) {
                focusPaused = false;
                start();
            }
        });
        element.addEventListener("touchstart", () => { hoverPaused = true; }, { passive: true });
        element.addEventListener("touchend", () => {
            setTimeout(() => { hoverPaused = false; start(); }, 900);
        }, { passive: true });
        start();
    };

    const setAutoplayPaused = (paused) => {
        userPaused = !!paused;
        if (!userPaused) start();
    };

    return { scrollProducts, initProductAutoplay, setAutoplayPaused };
})();