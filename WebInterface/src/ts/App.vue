<template>
	<div id="app">
		<Navbar />
		<section class="section app-content">
			<div class="container app-container">
				<router-view />
			</div>
		</section>
	</div>
</template>

<script lang="ts">
import Navbar from "./Components/Navbar.vue";

export default {
	data() {
		return {};
	},
	created() {
		const theme = localStorage.getItem("theme");
		if (theme !== "light") {
			document.documentElement.classList.add("theme-dark");
		}
	},
	components: {
		Navbar,
	}
};
</script>

<style lang="less">
:root {
	--bg: #f4f7fb; --surface: rgba(255,255,255,.86); --surface-solid: #fff;
	--text: #142033; --muted: #64748b; --line: rgba(99,115,145,.16);
	--brand: #6d5dfc; --brand-2: #14b8a6; --shadow: 0 20px 50px rgba(37,48,78,.10);
}
html.theme-dark {
	--bg: #080d19; --surface: rgba(17,24,39,.82); --surface-solid: #111827;
	--text: #f1f5f9; --muted: #94a3b8; --line: rgba(148,163,184,.15);
	--shadow: 0 20px 55px rgba(0,0,0,.32);
}
html { background: var(--bg); transition: background .3s ease, color .3s ease; }
body { min-height: 100vh; background: var(--bg); color: var(--text); }
body:before { content:""; position:fixed; inset:0; pointer-events:none; background: radial-gradient(circle at 10% 5%,rgba(109,93,252,.14),transparent 28%),radial-gradient(circle at 90% 12%,rgba(20,184,166,.10),transparent 24%); }
#app { min-height:100vh; position:relative; }
.app-content { padding-top: 7.25rem; padding-bottom: 4rem; }
.app-container { max-width: 1240px !important; }
.title,.label,.modal-card-title,.card-header-title { color:var(--text) !important; }
.subtitle,.help,.menu-label { color:var(--muted) !important; }
.box,.card,.notification,.modal-card-body,.modal-card-head,.modal-card-foot,.dropdown-content,.panel-heading,.panel-block {
	background:var(--surface) !important; color:var(--text) !important; border-color:var(--line) !important; box-shadow:var(--shadow); backdrop-filter:blur(18px);
}
.box,.card,.notification,.modal-card { border:1px solid var(--line); border-radius:20px !important; }
.button,.input,.textarea,.select select { min-height:44px; border-radius:12px !important; transition:all .2s ease; }
.input,.textarea,.select select { background:var(--surface-solid) !important; border-color:var(--line) !important; color:var(--text) !important; box-shadow:none !important; }
.input:focus,.textarea:focus,.select select:focus { border-color:var(--brand) !important; box-shadow:0 0 0 3px rgba(109,93,252,.14) !important; }
.button { font-weight:700; border-color:var(--line); }
.buttons { gap:.65rem; }
.buttons .button { margin:0 !important; }
.field.is-grouped { gap:.55rem; }
.field.is-grouped > .control { margin-right:0 !important; }
.button:hover { transform:translateY(-2px); box-shadow:0 10px 24px rgba(31,41,55,.12); }
.button.is-primary,.pagination-link.is-current { background:linear-gradient(135deg,var(--brand),#8b5cf6) !important; border:0 !important; color:#fff !important; }
.button.is-success { background:linear-gradient(135deg,#10b981,var(--brand-2)) !important; border:0 !important; color:#fff !important; }
.table-wrapper { border:1px solid var(--line); border-radius:18px; overflow:auto; background:var(--surface); box-shadow:var(--shadow); }
.table { background:transparent !important; color:var(--text) !important; }
.table th { color:var(--muted) !important; border-color:var(--line) !important; text-transform:uppercase; font-size:.72rem; letter-spacing:.08em; }
.table td { border-color:var(--line) !important; vertical-align:middle !important; }
.table.is-hoverable tbody tr:hover { background:rgba(109,93,252,.07) !important; }
.tag { border-radius:999px !important; font-weight:700; padding-inline:.8rem; }
.tabs a { color:var(--muted); border-color:var(--line) !important; }
.tabs li.is-active a { color:var(--brand) !important; border-color:var(--brand) !important; }
.dropdown-item,.menu-list a,.panel-block { color:var(--text) !important; }
.dropdown-item:hover,.menu-list a:hover,.panel-block:hover { background:rgba(109,93,252,.08) !important; }
@keyframes rise { from { opacity:0; transform:translateY(18px); } to { opacity:1; transform:none; } }
.app-container > * { animation:rise .55s ease both; }
@media (prefers-reduced-motion:reduce) { *,*:before,*:after { scroll-behavior:auto !important; animation-duration:.01ms !important; transition-duration:.01ms !important; } }
@media (max-width:768px) {
	.app-content { padding:5.75rem 1rem 2.5rem; }
	.columns { margin-left:0; margin-right:0; }
	.column { padding:.6rem 0; }
	.buttons .button { flex:1; }
	.table-wrapper { border-radius:14px; }
}
</style>
