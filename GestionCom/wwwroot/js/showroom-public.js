window.fednadShowroom = (() => {
    let frameId = 0;
    let track = null;
    let lastTime = 0;
    let direction = 1;
    let userPaused = false;
    let focusPaused = false;
    let touchPaused = false;
    let listeners = null;
    const speed = 32;

    const isPaused = () => userPaused || focusPaused || touchPaused;

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

        const maxScroll = track.scrollWidth - track.clientWidth;
        if (!isPaused() && maxScroll > 2 && !document.hidden) {
            if (lastTime) {
                const elapsed = Math.min((time - lastTime) / 1000, 0.05);
                const next = track.scrollLeft + direction * speed * elapsed;
                if (next >= maxScroll) {
                    track.scrollLeft = maxScroll;
                    direction = -1;
                } else if (next <= 0) {
                    track.scrollLeft = 0;
                    direction = 1;
                } else {
                    track.scrollLeft = next;
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
        touchPaused = true;
        window.setTimeout(() => { touchPaused = false; start(); }, 1100);
    };

    const scrollProducts = (element, amount) => {
        if (!element) return;
        pauseForManualControl();
        const card = element.querySelector(".fn-product-card");
        const styles = window.getComputedStyle(element);
        const gap = parseFloat(styles.columnGap || styles.gap || "0");
        const distance = card ? card.getBoundingClientRect().width + gap : element.clientWidth * 0.8;
        element.scrollBy({ left: amount * distance, behavior: "smooth" });
        direction = amount > 0 ? 1 : -1;
    };

    const initProductAutoplay = (element, initiallyPaused) => {
        if (!element) return false;
        if (track === element && listeners) {
            start();
            return !userPaused;
        }

        stopFrame();
        if (track && listeners) {
            track.removeEventListener("focusin", listeners.focusin);
            track.removeEventListener("focusout", listeners.focusout);
            track.removeEventListener("touchstart", listeners.touchstart);
            track.removeEventListener("touchend", listeners.touchend);
        }

        track = element;
        userPaused = !!initiallyPaused;
        focusPaused = false;
        touchPaused = false;
        listeners = {
            focusin: () => { focusPaused = true; },
            focusout: (event) => {
                if (!element.contains(event.relatedTarget)) {
                    focusPaused = false;
                    start();
                }
            },
            touchstart: () => { touchPaused = true; },
            touchend: () => {
                window.setTimeout(() => { touchPaused = false; start(); }, 1100);
            }
        };
        Object.entries(listeners).forEach(([name, handler]) =>
            element.addEventListener(name, handler, { passive: name.startsWith("touch") }));

        start();
        return !userPaused;
    };

    const setAutoplayPaused = (paused) => {
        userPaused = !!paused;
        if (userPaused) stopFrame();
        else start();
    };

    return { scrollProducts, initProductAutoplay, setAutoplayPaused };
})();
