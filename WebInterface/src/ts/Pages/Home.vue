<template>
	<div class="home-page">
		<section class="home-hero">
			<div class="hero-copy">
				<div class="eyebrow"><span class="status-dot"></span> TeamSpeak audio, reimagined</div>
				<h1>Tu música.<br><span>Tu servidor.</span></h1>
				<p>Controlá bots, playlists y reproducción desde una experiencia rápida, clara y diseñada para cualquier pantalla.</p>
				<div class="hero-features">
					<span><b-icon icon="lightning-bolt" size="is-small"></b-icon> Instantáneo</span>
					<span><b-icon icon="shield-check" size="is-small"></b-icon> Seguro</span>
					<span><b-icon icon="cellphone" size="is-small"></b-icon> Responsive</span>
				</div>
			</div>
			<div class="login-card">
				<div class="login-icon"><b-icon icon="shield-key" size="is-medium"></b-icon></div>
				<div>
					<p class="eyebrow">Panel de control</p>
					<h2 class="title is-3">Bienvenido</h2>
					<p class="login-help">Ingresá tus credenciales para administrar tus bots.</p>
				</div>
				<div class="login-form">
					<b-field label="Client UID">
						<b-input :value="authUid" @input="authUidInput" placeholder="Tu identidad de TeamSpeak" icon="account" expanded />
					</b-field>
					<b-field label="Token de acceso">
						<b-input v-model="authToken" placeholder="Token generado con !api token" type="password" icon="key" password-reveal expanded @keyup.native.enter="login" />
					</b-field>
					<b-button type="is-primary" icon-left="login" expanded :loading="authenticating" :disabled="!canLogin" @click="login">
						{{ logged_in ? 'Ir al panel' : 'Iniciar sesión' }}
					</b-button>
				</div>
				<div class="auth-state" :class="{'is-ready': logged_in, 'has-error': authError}"><span></span>{{ authError || (logged_in ? 'Credenciales verificadas. Ya podés entrar.' : 'Tus datos se guardan localmente') }}</div>
			</div>
		</section>

		<div v-if="logged_in" class="quick-grid single">
				<router-link to="bots" class="quick-card">
					<div class="quick-icon teal"><b-icon icon="robot" size="is-medium"></b-icon></div>
					<div><strong>Administrar bots</strong><span>Conectá, configurá y reproducí</span></div><b-icon icon="arrow-right"></b-icon>
				</router-link>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from "vue";
import { jmerge, Get } from "../Api";
import { Util } from "../Util";
import { ApiAuth } from "../ApiAuth";

export default Vue.extend({
	data() {
		return {
			 authUid: "",
			authToken: "",
			logged_in: false,
			authenticating: false,
			authError: ""
		};
	},
	created() {
		this.authStr = Get.AuthData.getFullAuth();
		if (this.authUid && this.authToken) this.authenticate();
	},
	computed: {
		canLogin(): boolean {
			return this.authUid.trim().length > 0 && this.authToken.trim().length > 0;
		},
		authStr: {
			get(): string {
				return this.authUid + ":" + this.authToken;
			},
			set(val: string) {
				if (!val.includes(":")) {
					this.authUid = val;
					this.authToken = "";
				} else {
					const split = val.split(":");
					this.authUid = split[0].replace(/:/g, "");
					this.authToken = split[1];
				}
			}
		}
	},
	methods: {
		authUidInput(val: string) {
			if (!val.includes(":")) {
				this.authUid = val;
			} else {
				const separator = val.indexOf(":");
				this.authUid = val.substring(0, separator).replace(/:/g, "").trim();
				this.authToken = val.substring(separator + 1).trim();
			}
			this.logged_in = false;
			this.authError = "";
		},
		async authenticate(): Promise<boolean> {
			if (!this.canLogin || this.authenticating) return false;
			this.authenticating = true;
			this.authError = "";
			Get.AuthData = ApiAuth.Create(this.authStr);
			window.localStorage.setItem("api_auth", Get.AuthData.getFullAuth());
			try {
				const res = await jmerge().get();
				this.logged_in = Util.check(this, res, "No se pudieron validar las credenciales");
				if (this.logged_in) window.dispatchEvent(new Event("dixibot-auth"));
				if (!this.logged_in) this.authError = "UID o token incorrectos";
				return this.logged_in;
			} catch (_) {
				this.logged_in = false;
				this.authError = "No se pudo conectar con el bot";
				return false;
			} finally {
				this.authenticating = false;
			}
		},
		async login() {
			if ((this.logged_in || await this.authenticate())) this.$router.push("/bots");
		}
	}
});
</script>

<style lang="less">
.home-page{margin-top:.5rem}.home-hero{display:grid;grid-template-columns:1.15fr .85fr;gap:clamp(2rem,6vw,6rem);align-items:center;min-height:62vh}.hero-copy h1{font-size:clamp(3.2rem,7vw,6.3rem);line-height:.92;letter-spacing:-.065em;color:var(--text);font-weight:850;margin:1.2rem 0 1.6rem}.hero-copy h1 span{background:linear-gradient(100deg,var(--brand),var(--brand-2));-webkit-background-clip:text;color:transparent}.hero-copy>p{font-size:1.15rem;line-height:1.75;color:var(--muted);max-width:650px}.eyebrow{text-transform:uppercase;letter-spacing:.12em;font-size:.72rem;font-weight:800;color:var(--brand)}.status-dot{display:inline-block;width:8px;height:8px;border-radius:50%;margin-right:.5rem;background:var(--brand-2);box-shadow:0 0 0 6px rgba(20,184,166,.12);animation:pulse 2s infinite}.hero-features{display:flex;flex-wrap:wrap;gap:1rem;margin-top:2rem}.hero-features span{display:flex;align-items:center;gap:.4rem;color:var(--muted);font-weight:650}.login-card{position:relative;padding:2.2rem;border:1px solid var(--line);border-radius:28px;background:var(--surface);box-shadow:var(--shadow);backdrop-filter:blur(22px)}.login-icon{display:flex;width:54px;height:54px;align-items:center;justify-content:center;border-radius:16px;background:linear-gradient(135deg,var(--brand),#8b5cf6);color:#fff;margin-bottom:1.8rem;box-shadow:0 12px 30px rgba(109,93,252,.3)}.login-card .title{margin:.35rem 0 .5rem!important}.login-help{color:var(--muted);margin-bottom:1.7rem}.login-form .field{margin-bottom:1rem}.login-form>.button{margin-top:.35rem}.auth-state{display:flex;align-items:center;gap:.5rem;color:var(--muted);font-size:.8rem;margin-top:1.25rem}.auth-state span{width:7px;height:7px;border-radius:50%;background:var(--muted)}.auth-state.is-ready{color:#10b981}.auth-state.is-ready span{background:#10b981}.auth-state.has-error{color:#f14668}.auth-state.has-error span{background:#f14668}.quick-grid{display:grid;grid-template-columns:repeat(2,1fr);gap:1rem;margin-top:2rem}.quick-card{display:flex;align-items:center;gap:1rem;padding:1.2rem;border:1px solid var(--line);border-radius:18px;background:var(--surface);color:var(--text);transition:.25s ease}.quick-card:hover{color:var(--brand);transform:translateY(-4px);box-shadow:var(--shadow)}.quick-card>div:nth-child(2){display:flex;flex:1;flex-direction:column}.quick-card span{font-size:.82rem;color:var(--muted)}.quick-icon{display:flex;align-items:center;justify-content:center;width:48px;height:48px;border-radius:14px;background:rgba(109,93,252,.12);color:var(--brand)}.quick-icon.teal{background:rgba(20,184,166,.12);color:var(--brand-2)}@keyframes pulse{50%{box-shadow:0 0 0 10px rgba(20,184,166,0)}}@media(max-width:860px){.home-hero{grid-template-columns:1fr;min-height:auto}.hero-copy{text-align:center}.hero-copy>p{margin-inline:auto}.hero-features{justify-content:center}.login-card{max-width:600px;width:100%;margin:auto}.quick-grid{grid-template-columns:1fr}}@media(max-width:480px){.hero-copy h1{font-size:3.2rem}.login-card{padding:1.4rem}.hero-features{gap:.65rem;font-size:.82rem}.quick-card{padding:1rem}}
</style>
