<template>
	<div class="playlists-page">
		<div v-if="editMode">
			<router-link class="back-link" :to="{ name: 'r_playlists', params: { playlist: '<none>' } }"><b-icon icon="arrow-left" size="is-small"></b-icon> Volver a playlists</router-link>
			<playlist-editor
				:botId="botId"
				:selectedPlaylist="selectedPlaylist"
				@deletePlaylist="deletePlaylistClick"
				@startPlaylist="startPlaylist"
			/>
		</div>
		<div v-else>
			<div class="playlist-header"><div><span class="page-kicker">Biblioteca</span><h2 class="title is-3">Playlists</h2><p>Organizá tu música y reproducila con un solo clic.</p></div><b-button type="is-primary" icon-left="plus" @click="modalCreatePlaylist = true">Nueva playlist</b-button></div>
			<b-field class="playlist-search"><b-input icon="magnify" v-model="filter" placeholder="Buscar playlist..." expanded /></b-field>

			<div class="playlist-container">
				<div class="playlist-card create-card" @click="modalCreatePlaylist = true">
					<div class="pc-content">
						<a class="square-box shadow-box">
							<div class="pc-center">
								<b-icon icon="plus" size="is-large" />
							</div>
						</a>
						<p class="card-title title is-5">Crear playlist</p>
					</div>
				</div>

				<div v-for="list in playlists_filter" :key="list.Id" class="playlist-card">
					<div class="pc-content">
						<div class="square-box shadow-box playlist-cover">
							<div class="pc-back">
								<router-link :to="{ name: 'r_playlists', params: { 'playlist': list.Id }}">
									<v-canvas :width="5" :height="5" @draw="genImage(list.Id, $event)" />
									<!-- <img src="https://bulma.io/images/placeholders/128x128.png" /> -->
								</router-link>
							</div>
							<div class="pc-center pc-hover">
								<a
									class="button is-large is-primary"
									style="border-radius: 100%;"
									@click="startPlaylist(list.Id)"
								>
									<b-icon icon="play" size="is-large" />
								</a>
							</div>
						</div>
						<p class="card-title title is-5">{{ list.Title || list.Id }}</p><p class="playlist-id">{{ list.Id }}</p>
					</div>
				</div>
			</div>
		</div>

		<b-modal :active.sync="modalCreatePlaylist">
			<CreatePlaylistModal
				@callback="createPlaylist"
				:existingFiles="playlists.map(x => x.Id.toLowerCase())"
			/>
		</b-modal>
	</div>
</template>

<script lang="ts">
import Vue from "vue";
import PlaylistEditor from "../Components/PlaylistEditor.vue";
import VCanvas from "../Components/VCanvas.vue";
import { bot, cmd } from "../Api";
import { CmdPlaylistInfo } from "../ApiObjects";
import { Util } from "../Util";
import CreatePlaylistModal from "../Modals/CreatePlaylistModal.vue";
import DeletePlaylistModal from "../Modals/DeletePlaylistModal.vue";

let nextTodoId = 1;

export default Vue.extend({
	components: {
		VCanvas,
		PlaylistEditor,
		CreatePlaylistModal,
		DeletePlaylistModal
	},
	data() {
		return {
			filter: "",
			playlists: [] as CmdPlaylistInfo[],

			modalCreatePlaylist: false,
			modalDeletePlaylist: false
		};
	},
	async created() {
		await this.refresh();
	},
	methods: {
		async refresh() {
			const res = await bot(
				cmd<CmdPlaylistInfo[]>("list", "list"),
				this.botId
			).get();

			if (!Util.check(this, res, "Failed to retrieve playlists")) {
				return;
			}

			this.playlists = res;
		},
		async createPlaylist(listId: string, title: string) {
			this.modalCreatePlaylist = false;

			const res = await bot(
				cmd<void>("list", "create", listId, title),
				this.botId
			).get();

			if (!Util.check(this, res, "Failed to create playlist")) {
				return;
			}

			this.$router.push({
				name: "r_playlists",
				params: { playlist: listId }
			});

			// update playlists in the background
			await this.refresh();
		},
		async deletePlaylistClick(list: CmdPlaylistInfo) {
			this.modalDeletePlaylist = true;

			this.$buefy.modal.open({
				parent: this,
				component: DeletePlaylistModal,
				hasModalCard: true,
				props: {
					list: list
				},
				events: {
					callback: this.deletePlaylist
				}
			});
		},
		async deletePlaylist(list: CmdPlaylistInfo) {
			this.modalDeletePlaylist = false;

			const res = await bot(
				cmd<void>("list", "delete", list.Id),
				this.botId
			).get();

			if (!Util.check(this, res, "Failed to delete playlist")) {
				return;
			}

			this.$router.push({
				name: "r_playlists",
				params: { playlist: "<none>" }
			});

			// update playlists in the background
			await this.refresh();
		},
		async startPlaylist(fileName: string) {
			const res = await bot(
				cmd<void>("list", "play", fileName),
				this.botId
			).get();

			if (!Util.check(this, res, "Failed to start playlist")) {
				return;
			}

			this.$emit("requestRefresh");
		},
		genImage(name: string, ev: any) {
			Util.genImage(name, ev.ctx, ev.width, ev.height);
		}
	},
	computed: {
		botId(): number {
			return Number(this.$route.params.id);
		},
		selectedPlaylist(): string {
			return this.$route.params.playlist;
		},
		editMode(): boolean {
			return this.selectedPlaylist != "<none>";
		},
		playlists_filter(): CmdPlaylistInfo[] {
			if (this.filter.length == 0) return this.playlists;
			const lc_filter = this.filter.toLowerCase();
			return this.playlists.filter(
				list =>
					list.Id.toLowerCase().includes(lc_filter) ||
					(list.Title && list.Title.toLowerCase().includes(lc_filter))
			);
		}
	},
	watch: {
		editMode(val: boolean) {
			if (!val) this.refresh();
		}
	}
});
</script>

<style lang="less">
.playlist-header{display:flex;align-items:flex-end;justify-content:space-between;gap:1rem;margin-bottom:1.25rem}.playlist-header .title{margin:.25rem 0!important}.playlist-header p,.playlist-id{color:var(--muted)}.playlist-search{max-width:520px;margin-bottom:1.4rem!important}.playlist-container{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:1rem}.playlist-card{min-width:0;cursor:pointer}.pc-content{padding:.8rem!important;border:1px solid transparent;border-radius:18px;transition:.25s ease}.pc-content:hover{background:var(--surface);border-color:var(--line);transform:translateY(-4px);box-shadow:var(--shadow)}.playlist-id{text-align:center;font-size:.72rem;margin-top:-.65rem;overflow:hidden;text-overflow:ellipsis}.back-link{display:inline-flex;align-items:center;gap:.4rem;margin-bottom:1rem;color:var(--muted)}.back-link:hover{color:var(--brand)}

.square-box {
	display: block;
	position: relative;

	&:after {
		content: "";
		display: block;
		padding-bottom: 100%;
	}
}

.shadow-box {
	border-radius:16px;overflow:hidden;background:linear-gradient(145deg,rgba(109,93,252,.15),rgba(20,184,166,.12));border:1px solid var(--line);
	&:hover {
		box-shadow:0 16px 34px rgba(15,23,42,.16);
	}
	&:not(:hover) {
		.pc-hover {
			display: none;
		}
	}
}

.pc-content {
	display: block;
	padding: 0.5em;
}

.fill() {
	left: 0;
	top: 0;
	right: 0;
	bottom: 0;

	pointer-events: none;

	> * {
		pointer-events: auto;
	}
}

.pc-back {
	.fill();
	position: absolute;
}

.pc-center {
	.fill();
	display: flex;
	position: absolute;
	justify-content: center;
	align-items: center;
}

.pc-bottom {
	.fill();
	display: flex;
	position: absolute;
	justify-content: center;
	align-items: end;
}

.card-title {
	text-overflow: ellipsis;
	white-space: nowrap;
	overflow: hidden;
	padding: 0.5em 0;
	text-align: center;
}
@media(max-width:900px){.playlist-container{grid-template-columns:repeat(3,minmax(0,1fr))}}@media(max-width:600px){.playlist-container{grid-template-columns:repeat(2,minmax(0,1fr))}.playlist-header{align-items:flex-start;flex-direction:column}.playlist-header .button{width:100%}}
</style>
