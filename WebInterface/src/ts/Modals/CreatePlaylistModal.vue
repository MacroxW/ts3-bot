<template>
	<form class="playlist-modal-form" @submit.prevent="trySubmit">
		<div class="modal-card playlist-modal-card">
			<header class="modal-card-head">
				<div class="modal-heading"><span class="modal-icon"><b-icon icon="playlist-plus"></b-icon></span><div><p class="modal-card-title">Nueva playlist</p><small>Creá una colección para organizar tu música.</small></div></div>
			</header>
			<section class="modal-card-body">
				<b-field label="Nombre de la playlist">
					<b-input v-model="title" icon="playlist-music" placeholder="Ej. Música para jugar" maxlength="80" required v-focus></b-input>
				</b-field>
				<b-field
					label="Identificador del archivo (opcional)"
					:type="is_taken ? 'is-danger' : ''"
					:message="is_taken ? 'Ya existe una playlist con este identificador.' : 'Se completa automáticamente a partir del nombre.'"
				>
					<div class="file-id-control"><b-input v-model="id" icon="file-outline" :placeholder="autoId || 'mi_playlist'" expanded></b-input><span>.ts3ablist</span></div>
				</b-field>
			</section>
			<footer class="modal-card-foot">
				<button class="button" type="button" @click="$parent.close()">Cancelar</button>
				<button class="button is-primary" type="submit" :disabled="autoId.length === 0 || is_taken"><b-icon icon="check"></b-icon><span>Crear playlist</span></button>
			</footer>
		</div>
	</form>
</template>

<script lang="ts">
import Vue from "vue";

export default Vue.extend({
	props: {
		existingFiles: {
			type: Array as () => string[],
			required: false,
			default: []
		}
	},
	data() {
		return {
			title: "",
			id: ""
		};
	},
	computed: {
		autoId(): string {
			if (this.id.length > 0) {
				return this.id;
			}
			return this.title
				.replace(/\s/g, "_")
				.replace(/[^\w-_]/g, "")
				.substring(0, 64);
		},
		is_taken(): boolean {
			return this.existingFiles.includes(this.autoId.toLowerCase());
		}
	},
	methods: {
		trySubmit() {
			if (this.autoId.length == 0 || this.is_taken) return;
			this.$emit("callback", this.autoId, this.title);
		}
	}
});
</script>

<style lang="less">
.playlist-modal-card{width:min(92vw,560px)!important}.modal-heading{display:flex;align-items:center;gap:.9rem}.modal-heading small{display:block;color:var(--muted);margin-top:.2rem}.modal-icon{display:grid;place-items:center;width:44px;height:44px;border-radius:13px;background:rgba(109,93,252,.12);color:var(--brand)}.file-id-control{display:flex;width:100%;align-items:stretch}.file-id-control>.control{flex:1}.file-id-control span{display:flex;align-items:center;padding:0 1rem;border:1px solid var(--line);border-left:0;border-radius:0 12px 12px 0;background:rgba(100,116,139,.08);color:var(--muted);font-size:.82rem;font-weight:700}.file-id-control .input{border-radius:12px 0 0 12px!important}.playlist-modal-card .modal-card-foot{justify-content:flex-end;gap:.65rem}@media(max-width:520px){.playlist-modal-card .modal-card-foot{flex-direction:column-reverse}.playlist-modal-card .modal-card-foot .button{width:100%;margin:0}.file-id-control span{padding:0 .6rem;font-size:.72rem}}
</style>
