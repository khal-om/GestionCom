window.fednadShowroom = (() => {
    let frameId = 0;
    let track = null;
    let lastTime = 0;
    let direction = 1;
    let userPaused = false;
    let hoverPaused = false;
    let focusPaused = false;
    let reducedMotion = false;
    let motionOverride = false;
    let resumeTimer = 0;
    let listeners = null;
    const speed = 24;

    const isPaused = () =>
        userPaused || hoverPaused || focusPaused || (reducedMotion && !motionOverride);

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
                track.scrollLeft += direction * speed * elapsed;

                if (track.scrollLeft >= maxScroll - 1) {
                    track.scrollLeft = maxScroll;
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
        if (!element) return false;
        if (track === element && listeners) return !isPaused();

        stopFrame();
        if (track && listeners) {
            track.removeEventListener("pointerenter", listeners.pointerenter);
            track.removeEventListener("pointerleave", listeners.pointerleave);
            track.removeEventListener("focusin", listeners.focusin);
            track.removeEventListener("focusout", listeners.focusout);
            track.removeEventListener("touchstart", listeners.touchstart);
            track.removeEventListener("touchend", listeners.touchend);
        }

        track = element;
        userPaused = !!initiallyPaused;
        hoverPaused = false;
        focusPaused = false;
        reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
        motionOverride = false;

        listeners = {
            pointerenter: () => { hoverPaused = true; },
            pointerleave: () => { hoverPaused = false; start(); },
            focusin: () => { focusPaused = true; },
            focusout: (event) => {
                if (!element.contains(event.relatedTarget)) {
                    focusPaused = false;
                    start();
                }
            },
            touchstart: () => { hoverPaused = true; },
            touchend: () => {
                setTimeout(() => { hoverPaused = false; start(); }, 900);
            }
        };

        Object.entries(listeners).forEach(([name, handler]) => {
            element.addEventListener(name, handler, { passive: name.startsWith("touch") });
        });

        start();
        return !isPaused();
    };

    const setAutoplayPaused = (paused) => {
        userPaused = !!paused;
        if (!userPaused) motionOverride = true;
        if (userPaused) stopFrame();
        else start();
    };

    return { scrollProducts, initProductAutoplay, setAutoplayPaused };
})();
