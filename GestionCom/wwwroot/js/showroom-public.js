window.fednadShowroom = (() => {
    let frame = 0;
    let track = null;
    let previousTime = 0;
    let direction = 1;
    let pausedByUser = false;
    let pausedByFocus = false;
    let resizeObserver = null;
    const speed = 42; // pixels per second

    const stop = () => {
        if (frame) window.cancelAnimationFrame(frame);
        frame = 0;
        previousTime = 0;
    };

    const step = (time) => {
        if (!track || !track.isConnected) { stop(); return; }
        const max = track.scrollWidth - track.clientWidth;
        if (!pausedByUser && !pausedByFocus && max > 4 && !document.hidden) {
            if (previousTime) {
                const delta = Math.min((time - previousTime) / 1000, 0.05) * speed;
                let next = track.scrollLeft + direction * delta;
                if (next >= max) { next = max; direction = -1; }
                if (next <= 0) { next = 0; direction = 1; }
                track.scrollLeft = next;
            }
            previousTime = time;
        } else {
            previousTime = 0;
        }
        frame = window.requestAnimationFrame(step);
    };

    const start = () => {
        if (!frame && track) frame = window.requestAnimationFrame(step);
    };

    const initProductAutoplay = (element, initiallyPaused) => {
        if (!element) return false;
        if (track !== element) {
            stop();
            if (resizeObserver) resizeObserver.disconnect();
            track = element;
            pausedByUser = !!initiallyPaused;
            pausedByFocus = false;
            element.style.width = "100%";
            element.style.maxWidth = "100%";
            element.style.minWidth = "0";
            element.style.overflowX = "auto";
            element.style.flexWrap = "nowrap";
            Array.from(element.children).forEach(card => {
                card.style.flexShrink = "0";
            });
            element.addEventListener("focusin", () => { pausedByFocus = true; });
            element.addEventListener("focusout", event => {
                if (!element.contains(event.relatedTarget)) {
                    pausedByFocus = false;
                    start();
                }
            });
            resizeObserver = new ResizeObserver(() => start());
            resizeObserver.observe(element);
            if (element.firstElementChild) resizeObserver.observe(element.firstElementChild);
        }
        start();
        return !pausedByUser;
    };

    const setAutoplayPaused = (paused) => {
        pausedByUser = !!paused;
        if (pausedByUser) stop();
        else start();
    };

    const scrollProducts = (element, amount) => {
        if (!element) return;
        const card = element.querySelector(".fn-product-card");
        const styles = window.getComputedStyle(element);
        const gap = parseFloat(styles.columnGap || styles.gap || "0");
        const distance = card ? card.getBoundingClientRect().width + gap : element.clientWidth * .8;
        element.scrollBy({ left: amount * distance, behavior: "smooth" });
        direction = amount > 0 ? 1 : -1;
    };

    return { initProductAutoplay, setAutoplayPaused, scrollProducts };
})();
