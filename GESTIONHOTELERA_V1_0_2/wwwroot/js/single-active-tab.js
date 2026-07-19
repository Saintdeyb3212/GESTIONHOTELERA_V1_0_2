(() => {
    "use strict";

    const body = document.body;
    if (!body || body.dataset.authenticated !== "true") {
        return;
    }

    const storageKey = "hotel-walay-wanka.active-tab.v1";
    const channelName = "hotel-walay-wanka.session-tabs.v1";
    const heartbeatMilliseconds = 2500;
    const staleAfterMilliseconds = 10000;

    const tabId = typeof crypto !== "undefined" && crypto.randomUUID
        ? crypto.randomUUID()
        : `${Date.now()}-${Math.random().toString(16).slice(2)}`;

    const startedAt = Date.now();
    let transferred = false;
    let heartbeatHandle = 0;
    let channel = null;

    const buildClaim = () => ({
        type: "claim",
        tabId,
        startedAt,
        heartbeatAt: Date.now()
    });

    const parseClaim = value => {
        if (!value) {
            return null;
        }

        try {
            const claim = typeof value === "string" ? JSON.parse(value) : value;
            if (!claim || claim.type !== "claim" || !claim.tabId) {
                return null;
            }
            return claim;
        } catch {
            return null;
        }
    };

    const isIncomingNewer = claim => {
        if (!claim || claim.tabId === tabId) {
            return false;
        }

        if (claim.startedAt !== startedAt) {
            return claim.startedAt > startedAt;
        }

        return String(claim.tabId) > String(tabId);
    };

    const moveThisTabAside = () => {
        if (transferred) {
            return;
        }

        transferred = true;
        window.clearInterval(heartbeatHandle);

        try {
            channel?.close();
        } catch {
            // No action is needed if BroadcastChannel is unavailable.
        }

        // Browsers only allow window.close() for tabs opened by script. When it
        // is blocked, the old tab is moved to a neutral page without modules.
        try {
            window.close();
        } catch {
            // The fallback below handles user-opened tabs.
        }

        window.setTimeout(() => {
            if (!window.closed) {
                window.location.replace("/session-transferred.html");
            }
        }, 80);
    };

    const handleClaim = rawClaim => {
        const claim = parseClaim(rawClaim);
        if (isIncomingNewer(claim)) {
            moveThisTabAside();
        }
    };

    const publishClaim = () => {
        if (transferred) {
            return;
        }

        const claim = buildClaim();
        try {
            localStorage.setItem(storageKey, JSON.stringify(claim));
        } catch {
            // The BroadcastChannel path can still coordinate the tabs.
        }

        try {
            channel?.postMessage(claim);
        } catch {
            // localStorage remains as the fallback.
        }
    };

    window.addEventListener("storage", event => {
        if (event.key === storageKey) {
            handleClaim(event.newValue);
        }
    });

    if ("BroadcastChannel" in window) {
        channel = new BroadcastChannel(channelName);
        channel.addEventListener("message", event => handleClaim(event.data));
    }

    document.addEventListener("visibilitychange", () => {
        if (document.visibilityState !== "visible" || transferred) {
            return;
        }

        let activeClaim = null;
        try {
            activeClaim = parseClaim(localStorage.getItem(storageKey));
        } catch {
            // If storage is unavailable, this tab simply republishes its claim.
        }

        const activeHeartbeat = Number(activeClaim?.heartbeatAt ?? 0);
        const activeIsFresh = Date.now() - activeHeartbeat < staleAfterMilliseconds;

        if (activeClaim && activeClaim.tabId !== tabId && activeIsFresh) {
            if (isIncomingNewer(activeClaim)) {
                moveThisTabAside();
            }
            return;
        }

        publishClaim();
    });

    publishClaim();
    heartbeatHandle = window.setInterval(publishClaim, heartbeatMilliseconds);
})();
