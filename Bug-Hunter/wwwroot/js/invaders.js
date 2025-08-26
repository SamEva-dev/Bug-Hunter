console.log("[invaders] module loaded");

let raf = null, last = 0, dotnet = null, canvas = null, ctx = null;

function bg() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = "#0e0e10";
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.strokeStyle = "#1f1f22"; ctx.lineWidth = 1;
    for (let y = 0; y < canvas.height; y += 20) {
        ctx.beginPath(); ctx.moveTo(0, y + .5); ctx.lineTo(canvas.width, y + .5); ctx.stroke();
    }
}

function hud(s) {
    ctx.fillStyle = "white";
    ctx.font = "18px ui-monospace, Menlo, Consolas, monospace";
    ctx.fillText(`Score: ${s.score}`, 16, 24);
    ctx.fillText(`Lives: ${s.lives}`, 16, 46);
}

function rect(e, color) {
    ctx.fillStyle = color;
    ctx.fillRect(e.x, e.y, e.w, e.h);
}

function step(ts) {
    if (!last) last = ts;
    const dt = (ts - last) / 1000;
    last = ts;

    dotnet.invokeMethodAsync('Tick', dt).then(s => {
        bg();

        // Player
        rect(s.player, "#60a5fa");

        // Invaders
        for (const inv of s.invaders) {
            if (inv.alive) rect(inv, "#fbbf24");
        }

        // Shots (heuristique simple de couleur)
        for (const sh of s.shots) {
            const isFromPlayer = sh.y < s.player.y; // tir vers le haut = joueur
            rect(sh, isFromPlayer ? "#f87171" : "#34d399");
        }

        hud(s);

        if (s.running) {
            raf = requestAnimationFrame(step);
        } else {
            ctx.fillStyle = "white";
            ctx.font = "24px ui-monospace, Menlo, Consolas, monospace";
            ctx.fillText("Game Over", canvas.width / 2 - 70, canvas.height / 2);
            console.log("[invaders] stopped");
        }
    }).catch(err => {
        console.error("[invaders] Tick failed:", err);
        if (raf) cancelAnimationFrame(raf);
    });
}

function onKey(e) {
    if (!dotnet) return;
    const k = e.key.toLowerCase();
    if (k === "arrowleft" || k === "a") dotnet.invokeMethodAsync('Move', -1);
    if (k === "arrowright" || k === "d") dotnet.invokeMethodAsync('Move', +1);
    if (k === " " || k === "spacebar") dotnet.invokeMethodAsync('Shoot');
    if (k === "p") dotnet.invokeMethodAsync('TogglePause');
}

export async function startInvaders(dotnetRef, canvasRef) {
    console.log("[invaders] start()");
    dotnet = dotnetRef;
    canvas = canvasRef;
    ctx = canvas.getContext('2d');

    window.addEventListener('keydown', onKey);

    try {
        await dotnet.invokeMethodAsync('Start');
    } catch (e) {
        console.error("[invaders] .NET Start failed:", e);
    }

    last = 0;
    raf = requestAnimationFrame(step);
}

export function stopInvaders() {
    console.log("[invaders] stop()");
    if (raf) cancelAnimationFrame(raf);
    raf = null; last = 0;
    window.removeEventListener('keydown', onKey);
}
