<template>
	<form @submit.prevent="submit" action>
		<div class="modal-card" style="width: auto">
			<header class="modal-card-head">
				<p class="modal-card-title">Conexión rápida</p>
			</header>
			<section class="modal-card-body">
				<b-field label="IP / Dominio / ts3server://">
					<b-input v-model="address" placeholder="ej: ts3.teamspeak.com o ts3server://..." required v-focus></b-input>
				</b-field>
			</section>
			<footer class="modal-card-foot">
				<button class="button" type="button" @click="$parent.close()">Cerrar</button>
				<button class="button is-primary" type="submit">Conectar</button>
			</footer>
		</div>
	</form>
</template>

<script lang="ts">
import Vue from "vue";
import { QuickConnectData } from "../ApiObjects";

function parseTs3ServerUrl(url: string): QuickConnectData | null {
	try {
		const ts3url = url.replace(/^ts3server:\/\//i, "https://");
		const parsed = new URL(ts3url);
		const data: QuickConnectData = {
			address: parsed.hostname + (parsed.port ? ":" + parsed.port : ""),
		};
		const channel = parsed.searchParams.get("channel");
		if (channel) data.channel = channel;
		const channelPassword = parsed.searchParams.get("channelpassword");
		if (channelPassword) data.channelPassword = channelPassword;
		const serverPassword = parsed.searchParams.get("serverpassword");
		if (serverPassword) data.password = serverPassword;
		return data;
	} catch {
		return null;
	}
}

export default Vue.extend({
	data() {
		return {
			address: ""
		};
	},
	methods: {
		submit() {
			const input = this.address.trim();
			if (input.startsWith("ts3server://")) {
				const parsed = parseTs3ServerUrl(input);
				if (parsed) {
					this.$emit("callback", parsed);
					return;
				}
			}
			this.$emit("callback", { address: input } as QuickConnectData);
		}
	}
});
</script>
