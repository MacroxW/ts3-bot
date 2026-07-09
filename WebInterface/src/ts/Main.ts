// Modules
import Vue from "vue";
import VueRouter from "vue-router";
import Buefy from "buefy";
// Css
import "@mdi/font/css/materialdesignicons.css";
import "buefy/dist/buefy.css";
import "../less/styles_formdata.less";
// Pages
import App from "./App.vue";
import Bot from "./Pages/Bot.vue";
import Bots from "./Pages/Bots.vue";
import BotServer from "./Pages/BotServer.vue";
import BotSettings from "./Pages/BotSettings.vue";
import Home from "./Pages/Home.vue";
import Overview from "./Pages/Overview.vue";
import Playlists from "./Pages/Playlists.vue";
import Permissions from "./Pages/Permissions.vue";
import { Get } from "./Api";
import { ApiAuth } from "./ApiAuth";

Vue.use(VueRouter);
Vue.use(Buefy);
Vue.directive("focus", {
	inserted(el) {
		let ichild = el as HTMLInputElement;
		if (ichild.tagName !== "input") {
			ichild = el.querySelector("input")!;
		}

		if (ichild) {
			ichild.focus();
			ichild.setSelectionRange(0, ichild.value.length);
			return;
		}

		el.focus();
	}
});

const auth = window.localStorage.getItem("api_auth");
if (auth != null) {
	Get.AuthData = ApiAuth.Create(auth);
}

const router = new VueRouter({
	routes: [
		{ path: "/", component: Home },
		//{ path: "/openapi", component: Commands },
		{ path: "/overview", redirect: "/bots" },
		{ path: "/bots", component: Bots, name: "r_bots", meta: { requiresAuth: true } },
		{ path: "/permissions", component: Permissions, name: "r_permissions", meta: { requiresAuth: true } },
		{ path: "/bot", redirect: "/bots" },
		{ path: "/bot/", redirect: "/bots" },
		{
			path: "/bot/:id",
			component: Bot,
			meta: { requiresAuth: true },
			props: { online: true },
			children: [
				{ path: "", redirect: { name: "r_server" } },
				{
					name: "r_server",
					path: "server",
					component: BotServer
				},
				{
					name: "r_playlists",
					path: "playlists/:playlist",
					component: Playlists,
				},
				{
					name: "r_settings",
					path: "settings",
					component: BotSettings,
					props: { online: true }
				}
			]
		},
		{
			path: "/bot_offline/:name", component: Bot,
			meta: { requiresAuth: true },
			props: { online: false },
			children: [
				{
					name: "r_settings_offline",
					path: "settings",
					component: BotSettings,
					props: { online: false }
				},
			]
		},
		{ path: "*", redirect: "/" },
	]
});

router.beforeEach((to, from, next) => {
	const auth = window.localStorage.getItem("api_auth") || "";
	const authenticated = auth.includes(":") && auth.split(":")[1].length > 0;
	if (to.matched.some(route => route.meta.requiresAuth) && !authenticated) next("/");
	else next();
});

export default new Vue({
	el: "#app",
	template: "<App/>",
	components: {
		App
	},
	router
});
