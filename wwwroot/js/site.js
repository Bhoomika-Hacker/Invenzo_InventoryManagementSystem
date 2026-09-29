// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", function () {
 const widget=document.getElementById("zenoWidget"); if(!widget)return;
 const launcher=document.getElementById("zenoLauncher"), close=document.getElementById("zenoClose");
 const form=document.getElementById("zenoForm"), input=document.getElementById("zenoInput"), messages=document.getElementById("zenoMessages");
 launcher.onclick=()=>widget.classList.add("open"); close.onclick=()=>widget.classList.remove("open");
 document.querySelectorAll(".zeno-suggestions button").forEach(b=>b.onclick=()=>{input.value=b.dataset.q;form.dispatchEvent(new Event("submit",{bubbles:true,cancelable:true}));});
 form.onsubmit=async e=>{e.preventDefault();const text=input.value.trim();if(!text)return;
  const u=document.createElement("div");u.className="zeno-msg user";u.textContent=text;messages.appendChild(u);input.value="";
  const b=document.createElement("div");b.className="zeno-msg bot";b.textContent="Thinking…";messages.appendChild(b);
  try{const body=new URLSearchParams();body.append("message",text);body.append("__RequestVerificationToken",form.querySelector('[name="__RequestVerificationToken"]').value);
   const r=await fetch("/ZenoAI/Ask",{method:"POST",headers:{"Content-Type":"application/x-www-form-urlencoded"},body:body.toString()});const d=await r.json();b.innerHTML=d.reply||"Sorry, I couldn't process that.";
  }catch{b.textContent="I couldn't connect right now. Please try again."} messages.scrollTop=messages.scrollHeight;
 };
});

// =========================================================
// LIGHT / DARK MODE TOGGLE
// Applies the saved theme on every page load and wires up the
// switch on the Dashboard page to flip between the two modes.
// =========================================================
(function () {
    var STORAGE_KEY = "invenzo-theme";

    function applyTheme(theme) {
        if (theme === "dark") {
            document.body.classList.add("dark-mode");
        } else {
            document.body.classList.remove("dark-mode");
        }
    }

    // Apply saved preference immediately so every page (not just the
    // dashboard) respects the last chosen theme.
    var saved = localStorage.getItem(STORAGE_KEY) || "light";
    applyTheme(saved);

    document.addEventListener("DOMContentLoaded", function () {
        applyTheme(saved);

        var toggle = document.getElementById("themeToggle");
        if (!toggle) return;

        toggle.checked = saved === "dark";

        toggle.addEventListener("change", function () {
            var theme = toggle.checked ? "dark" : "light";
            applyTheme(theme);
            localStorage.setItem(STORAGE_KEY, theme);
        });
    });
})();
