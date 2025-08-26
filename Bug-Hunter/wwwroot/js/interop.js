console.log("[interop] module loaded");

let raf = null, last = 0, dotnet = null, canvas = null, ctx = null;
let sfx = null;

async function loadSfx() {
    if (sfx) return sfx;
    try {
        const click = new Audio(new URL('../sfx/click.wav', document.baseURI));
        const over = new Audio(new URL('../sfx/over.wav', document.baseURI));
        return (sfx = { click, over });
    } catch {
        return (sfx = {});
    }
}

function drawBackground() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = "#0e0e10";
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.strokeStyle = "#1f1f22";
    ctx.lineWidth = 1;
    for (let y = 0; y < canvas.height; y += 20) {
        ctx.beginPath();
        ctx.moveTo(0, y + .5);
        ctx.lineTo(canvas.width, y + .5);
        ctx.stroke();
    }
}

function drawHUD(s) {
    ctx.fillStyle = "white";
    ctx.font = "16px ui-monospace, Menlo, Consolas, monospace";
    ctx.fillText(`Score: ${s.score}`, 16, 24);
    ctx.fillText(`Combo: ${s.combo}`, 16, 46);
    ctx.fillText(`Time: ${s.time.toFixed(1)}s`, 16, 68);
}

function drawBugs(s) {
    for (const b of s.bugs) {
        ctx.beginPath();
        ctx.arc(b.x, b.y, b.r, 0, Math.PI * 2);
        ctx.fillStyle = b.rare ? "#f59e0b" : "#e11d48";
        ctx.fill();
        ctx.fillStyle = "#000";
        ctx.font = "bold 12px ui-monospace, Menlo, Consolas, monospace";
        const label = b.kind;
        const tw = ctx.measureText(label).width;
        ctx.fillText(label, b.x - tw / 2, b.y + 4);
    }
}


function drawParticles(s) {
    if (!s.particles) return;
    for (const p of s.particles) {
        const a = Math.max(0, Math.min(1, p.life / 0.4));
        ctx.fillStyle = `rgba(255,255,255,${a})`;
        ctx.fillRect(p.x, p.y, 2, 2);
    }
}

function step(ts) {
    if (!last) last = ts;
    const dt = (ts - last) / 1000;
    last = ts;

    dotnet.invokeMethodAsync('Tick', dt)
        .then(s => {
            drawBackground();
            drawBugs(s);
            drawParticles(s);
            drawHUD(s);

            if (s.running) {
                raf = requestAnimationFrame(step);
            } else {
                ctx.fillStyle = "white";
                ctx.font = "24px ui-monospace, Menlo, Consolas, monospace";
                ctx.fillText("Game Over", canvas.width / 2 - 70, canvas.height / 2);
                ctx.fillText(`Score: ${s.score}`, canvas.width / 2 - 60, canvas.height / 2 + 30);
                if (sfx?.over) sfx.over.play().catch(() => { });
                console.log("[interop] game stopped");
            }
        })
        .catch(err => {
            console.error("[interop] Tick failed:", err);
            if (raf) cancelAnimationFrame(raf);
        });
}

function onPointer(e) {
    if (!canvas) return;
    const r = canvas.getBoundingClientRect();
    const x = (e.clientX - r.left) * (canvas.width / r.width);
    const y = (e.clientY - r.top) * (canvas.height / r.height);
    dotnet.invokeMethodAsync('OnClick', x, y).then(() => {
        if (sfx?.click) sfx.click.play().catch(() => { });
    }).catch(err => console.error("[interop] OnClick failed:", err));
}

function onKey(e) {
    if (e.key.toLowerCase() === 'p') {
        dotnet.invokeMethodAsync('TogglePause').catch(() => { });
    }
}

export async function start(dotnetRef, canvasRef) {
    console.log("[interop] start()");
    dotnet = dotnetRef;
    canvas = canvasRef;
    ctx = canvas.getContext('2d');

    canvas.addEventListener('pointerdown', onPointer);
    window.addEventListener('keydown', onKey);

    await loadSfx();

    try {
        await dotnet.invokeMethodAsync('Start');
    } catch (err) {
        console.error("[interop] .NET Start failed:", err);
    }

    last = 0;
    raf = requestAnimationFrame(step);
}

export function stop() {
    console.log("[interop] stop()");
    if (raf) cancelAnimationFrame(raf);
    raf = null;
    last = 0;
    canvas?.removeEventListener('pointerdown', onPointer);
    window.removeEventListener('keydown', onKey);
}
