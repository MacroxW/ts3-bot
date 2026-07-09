<template>
	<b-navbar class="app-navbar is-spaced is-fixed-top">
		<template slot="brand">
			<b-navbar-item
				class="notification is-primary"
				id="ts3ab-logo"
				tag="router-link"
				:to="{ path: '/' }"
			><span class="brand-mark"><b-icon icon="waveform"></b-icon></span><span>Dixi<b>bot</b></span></b-navbar-item>
		</template>
		<template slot="start">
			<b-navbar-item v-if="authenticated" tag="router-link" :to="{ path: '/bots' }" active-class="is-active">
				<b-icon icon="robot"></b-icon>
				<span>Bots</span>
			</b-navbar-item>
			<b-navbar-item v-if="authenticated" tag="router-link" :to="{ path: '/permissions' }" active-class="is-active">
				<b-icon icon="shield-lock"></b-icon>
				<span>Permisos</span>
			</b-navbar-item>
		</template>

		<template slot="end">
			<b-tooltip label="Report a bug" position="is-bottom">
				<b-navbar-item
					tag="a"
					href="https://github.com/MacroxW/ts3-bot/issues/new/choose"
					target="_blank"
				>
					<b-icon icon="bug"></b-icon>
					<span class="is-hidden-desktop">Report a bug</span>
				</b-navbar-item>
			</b-tooltip>

			<b-tooltip label="Documentación del proyecto" position="is-bottom">
				<b-navbar-item
					tag="a"
					href="https://github.com/MacroxW/ts3-bot#readme"
					target="_blank"
				>
					<b-icon icon="account-multiple"></b-icon>
					<span class="is-hidden-desktop">Documentación</span>
				</b-navbar-item>
			</b-tooltip>

			<b-tooltip :label="isDark ? 'Switch to Light Mode' : 'Switch to Dark Mode'" position="is-bottom">
				<b-navbar-item tag="a" @click.native="toggleDarkMode()">
					<b-icon :icon="isDark ? 'weather-sunny' : 'weather-night'"></b-icon>
					<span class="is-hidden-desktop">{{ isDark ? 'Light Mode' : 'Dark Mode' }}</span>
				</b-navbar-item>
			</b-tooltip>

			<b-tooltip label="Site Settings" position="is-bottom">
				<b-navbar-item tag="a" @click.native="openSiteSettings()">
					<b-icon icon="cog"></b-icon>
					<span class="is-hidden-desktop">Site Settings</span>
				</b-navbar-item>
			</b-tooltip>

			<b-tooltip label="Login" position="is-bottom">
				<b-navbar-item tag="router-link" :to="{ path: '/' }">
					<b-icon icon="shield-key"></b-icon>
					<span class="is-hidden-desktop">Login</span>
				</b-navbar-item>
			</b-tooltip>
		</template>
	</b-navbar>
</template>

<script lang="ts">
import Vue from "vue";
import SiteSettingsModal from "../Modals/SiteSettingsModal.vue";

export default Vue.extend({
	data() {
		const auth = localStorage.getItem("api_auth") || "";
		return {
			isDark: localStorage.getItem("theme") !== "light",
			authenticated: auth.includes(":") && auth.split(":")[1].length > 0
		};
	},
	created() {
		this.applyTheme();
		window.addEventListener("dixibot-auth", this.syncAuth);
	},
	destroyed() {
		window.removeEventListener("dixibot-auth", this.syncAuth);
	},
	methods: {
		hasAuth(): boolean {
			const auth = localStorage.getItem("api_auth") || "";
			return auth.includes(":") && auth.split(":")[1].length > 0;
		},
		syncAuth() { this.authenticated = this.hasAuth(); },
		openSiteSettings() {
			this.$buefy.modal.open({
				parent: this,
				component: SiteSettingsModal,
				hasModalCard: false
			});
		},
		toggleDarkMode() {
			this.isDark = !this.isDark;
			localStorage.setItem("theme", this.isDark ? "dark" : "light");
			this.applyTheme();
		},
		applyTheme() {
			if (this.isDark) {
				document.documentElement.classList.add("theme-dark");
			} else {
				document.documentElement.classList.remove("theme-dark");
			}
		}
	},
	components: {
		SiteSettingsModal
	}
});
</script>

<style lang="less">
.app-navbar {
	top: 1rem !important; left: 50%; transform: translateX(-50%); width: calc(100% - 2rem); max-width: 1240px;
	min-height: 64px; border: 1px solid var(--line); border-radius: 18px; background: var(--surface) !important;
	box-shadow: 0 12px 40px rgba(15,23,42,.12); backdrop-filter: blur(20px); padding: 0 .65rem !important;
}
.app-navbar .navbar-item { color:var(--muted) !important; border-radius:12px; font-weight:650; gap:.45rem; }
.app-navbar .navbar-item:hover,.app-navbar .navbar-item.is-active { background:rgba(109,93,252,.09) !important; color:var(--brand) !important; }
#ts3ab-logo {
	margin-bottom:0; color:var(--text) !important; font-size:1.2rem; letter-spacing:-.03em;
}
.brand-mark { display:inline-flex; align-items:center; justify-content:center; width:36px; height:36px; margin-right:.55rem; border-radius:11px; color:#fff; background:linear-gradient(135deg,var(--brand),var(--brand-2)); box-shadow:0 8px 20px rgba(109,93,252,.28); }
@media(max-width:1023px){ .app-navbar { top:.5rem !important; width:calc(100% - 1rem); } .app-navbar .navbar-menu { background:var(--surface-solid) !important; border-radius:14px; margin-top:.6rem; padding:.5rem; box-shadow:var(--shadow); } }
</style>
