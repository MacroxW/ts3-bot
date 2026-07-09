<template>
	<div :class="{'has-playcontrols': online}" id="base-content">
		<nav class="flow-nav" aria-label="Ruta de navegación">
			<router-link to="/"><b-icon icon="home-outline" size="is-small"></b-icon> Inicio</router-link><b-icon icon="chevron-right" size="is-small"></b-icon><router-link to="/bots">Bots</router-link><b-icon icon="chevron-right" size="is-small"></b-icon><span>{{info.botInfo.Name || 'Detalle'}}</span>
		</nav>
		<header class="bot-detail-header">
			<div class="bot-identity"><div class="bot-avatar"><b-icon icon="robot" size="is-medium"></b-icon></div><div><span class="page-kicker">Panel del bot</span><h1 class="title is-2">{{info.botInfo.Name || 'Cargando bot...'}}</h1><p>{{info.botInfo.Server || 'Sin servidor configurado'}}</p></div></div>
			<div class="bot-status" :class="statusClass"><i></i>{{BotStatus[info.botInfo.Status]}}</div>
		</header>
		<div class="workflow-hint bot-flow"><span>2</span><div><strong>Administrá tu bot</strong><small>Elegí una sección, pegá un enlace para reproducir o ajustá su configuración.</small></div></div>
		<div class="columns bot-layout">
			<div class="column is-two-thirds">
				<div class="b-tabs bot-tabs">
					<div class="tabs is-boxed is-fullwidth">
						<ul>
							<bot-nav-item label="Server" icon="file-tree" page="r_server" :disabled="!online" />
							<bot-nav-item label="Settings" icon="cog" page="r_settings" />
							<bot-nav-item
								label="Playlists"
								icon="playlist-music"
								page="r_playlists"
								:props="{ playlist: '<none>' }"
								:disabled="!online"
							/>
							<bot-nav-item label="History [WIP]" icon="history" page disabled />
							<bot-nav-item label="Search [WIP]" icon="cloud-search" page disabled />
						</ul>
					</div>
				</div>
				<div class="container bot-view">
					<router-view @requestRefresh="refresh" />
				</div>
			</div>

			<div class="column is-one-third">
				<div class="notification bot-summary">
					<div class="card-label"><b-icon icon="information-outline"></b-icon> Resumen</div>
					<div class="formdatablock">
						<div>ID:</div>
						<div>{{info.botInfo.Id}}</div>
					</div>
					<div class="formdatablock">
						<div>Name:</div>
						<div>{{info.botInfo.Name}}</div>
					</div>
					<div class="formdatablock">
						<div>Server:</div>
						<div>{{info.botInfo.Server}}</div>
					</div>
					<div class="formdatablock">
						<div>Status:</div>
						<div>{{BotStatus[info.botInfo.Status]}}</div>
					</div>
				</div>

				<div class="box queue-card">
					<div class="card-label"><b-icon icon="music-note-plus"></b-icon> Reproducción rápida</div>
					<b-field class="song-entry">
						<b-input v-model="loadSongUrl" type="text" icon="link-variant" placeholder="Pegá un enlace de YouTube o audio" expanded @keyup.native.enter="playNewSong" />
					</b-field>
					<div class="song-actions">
						<b-button icon-left="playlist-plus" @click="addNewSong" expanded>Agregar a la cola</b-button>
						<b-button type="is-primary" icon-left="play" @click="playNewSong" expanded>Reproducir ahora</b-button>
					</div>

					<div v-if="info.song != null" class="media" style="margin-bottom: 1em;">
						<figure class="media-left">
							<p class="image is-64x64">
								<img :src="getCoverUrl()" />
							</p>
						</figure>
						<div class="media-content">
							<div class="field">
								<a :href="info.song.Link" target="_blank">
									<b-icon :icon="typeIcon(info.song.AudioType)" :style="colorIcon(info.song.AudioType)"></b-icon>
									<strong>{{info.song.Title}}</strong>
								</a>
							</div>
						</div>
					</div>

					<div class="title is-5 queue-title">A continuación</div>

					<b-table :data="info.nowPlaying.Items" style="margin-bottom: 1em;" hoverable>
						<template slot-scope="props">
							<b-table-column
								label=" "
								class="is-flex uni-hover"
								:class="{
									'is-selected': (info.nowPlaying.DisplayOffset + props.index) == info.nowPlaying.PlaybackIndex,
									'is-light': (info.nowPlaying.DisplayOffset + props.index) < info.nowPlaying.PlaybackIndex
								}"
							>
								<b-icon :icon="typeIcon(props.row.AudioType)" :style="colorIcon(props.row.AudioType)"></b-icon>
								<span>{{props.row.Title}}</span>
							</b-table-column>
						</template>
						<template slot="empty"><div class="queue-empty">La cola está vacía</div></template>
					</b-table>
				</div>
			</div>
		</div>

		<play-controls v-if="online" :botId="botId" :info="info" @requestRefresh="refresh" />
	</div>
</template>

<script lang="ts">
import Vue from "vue";
import PlayControls from "../Components/PlayControls.vue";
import BotNavbarItem from "../Components/BotNavbarItem.vue";
import {
	CmdBotInfo,
	CmdPlaylist,
	CmdSong,
	CmdQueueInfo,
	Empty
} from "../ApiObjects";
import { Get, bot, cmd, jmerge } from "../Api";
import { Util } from "../Util";
import { RepeatKind } from "../Model/RepeatKind";
import { BotStatus } from "../Model/BotStatus";
import { BotInfoSync } from "../Model/BotInfoSync";

export default Vue.extend({
	props: {
		online: { type: Boolean, required: true }
	},
	created() {
		this.refresh();
	},
	data() {
		return {
			BotStatus,

			loadSongUrl: "",

			info: new BotInfoSync()
		};
	},
	computed: {
		botId(): number {
			return Number(this.$route.params.id);
		},
		botName(): string {
			return this.$route.params.name;
		},
		statusClass(): string {
			if (this.info.botInfo.Status === BotStatus.Connected) return "is-online";
			if (this.info.botInfo.Status === BotStatus.Connecting) return "is-connecting";
			return "is-offline";
		}
	},
	methods: {
		track(val: any) {
			console.log(val);
			return val;
		},
		async refresh() {
			if (this.online) {
				const res = await bot(
					jmerge(
						cmd<CmdBotInfo>("bot", "info"),
						cmd<CmdQueueInfo>("info", "@-1", "5"),
						cmd<CmdSong | null>("song"),
						cmd<RepeatKind>("repeat"),
						cmd<boolean>("random"),
						cmd<number>("volume")
					),
					this.botId
				).get();

				if (!Util.check(this, res, "Failed to get bot information"))
					return;

				this.info.botInfo = res[0] ?? Empty.CmdBotInfo();
				this.info.nowPlaying = res[1] ?? Empty.CmdQueueInfo();
				this.info.song = res[2];
				this.info.repeat = res[3];
				this.info.shuffle = res[4];
				this.info.volume = Math.floor(res[5]);
			} else {
				this.info.botInfo.Id = ("N/A" as any) as number;
				this.info.botInfo.Name = this.botName;
				this.info.botInfo.Status = BotStatus.Offline;
			}
		},
		playNewSong() {
			return this.newSong("play");
		},
		addNewSong() {
			return this.newSong("add");
		},
		async newSong(action: string) {
			const song = this.loadSongUrl;
			this.loadSongUrl = "";
			const res = await bot(
				cmd<CmdBotInfo>(action, song),
				this.botId
			).get();

			if (!Util.check(this, res, "Failed to start song")) return;

			await this.refresh();
		},
		typeIcon: Util.typeIcon,
		colorIcon: Util.colorIcon,
		getCoverUrl(): string {
			return (
				Get.Endpoint +
				bot(cmd("data", "song", "cover", "get"), this.botId).done()
			);
		}
	},
	components: {
		PlayControls,
		BotNavbarItem
	}
});
</script>

<style lang="less">
#base-content.has-playcontrols{padding-bottom:4.5rem}.flow-nav{display:flex;align-items:center;gap:.4rem;margin-bottom:1.4rem;color:var(--muted);font-size:.82rem}.flow-nav a{display:flex;align-items:center;gap:.35rem;color:var(--muted)}.flow-nav a:hover{color:var(--brand)}.bot-detail-header{display:flex;align-items:center;justify-content:space-between;gap:1.5rem;margin-bottom:1.2rem}.bot-identity{display:flex;align-items:center;gap:1rem}.bot-avatar{display:grid;place-items:center;width:64px;height:64px;flex:none;border-radius:19px;background:linear-gradient(135deg,var(--brand),var(--brand-2));color:#fff;box-shadow:0 12px 30px rgba(109,93,252,.25)}.bot-detail-header .title{margin:.2rem 0!important;letter-spacing:-.04em}.bot-detail-header p{color:var(--muted)}.bot-status{display:flex;align-items:center;gap:.5rem;padding:.65rem .9rem;border-radius:999px;background:rgba(100,116,139,.1);color:var(--muted);font-weight:800;font-size:.78rem}.bot-status i{width:8px;height:8px;border-radius:50%;background:currentColor}.bot-status.is-online{color:#10b981;background:rgba(16,185,129,.1)}.bot-status.is-connecting{color:#f59e0b;background:rgba(245,158,11,.1)}.bot-status.is-offline{color:#f14668;background:rgba(241,70,104,.1)}.bot-flow{margin-bottom:1.5rem}.bot-layout{align-items:flex-start}.bot-tabs{padding:.6rem;background:var(--surface);border:1px solid var(--line);border-radius:18px;box-shadow:var(--shadow)}.bot-tabs .tabs{margin-bottom:0}.bot-view{margin-top:1rem}.bot-summary{padding:1.35rem!important}.bot-summary .formdatablock{justify-content:space-between;padding:.35rem 0;border-bottom:1px solid var(--line)}.bot-summary .formdatablock:last-child{border-bottom:0}.queue-card{padding:1.35rem}.song-entry{margin-bottom:.7rem!important}.song-actions{display:flex;gap:.65rem;margin-bottom:1.5rem}.queue-title{margin:1.2rem 0 .6rem!important}.queue-empty{text-align:center;color:var(--muted);padding:1rem}.media img{border-radius:12px}.page-kicker{color:var(--brand);font-size:.72rem;font-weight:800;letter-spacing:.12em;text-transform:uppercase}@media(max-width:768px){.bot-detail-header{align-items:flex-start}.bot-avatar{width:50px;height:50px;border-radius:15px}.bot-status{padding:.55rem}.bot-status:not(.is-online){font-size:0}.bot-status i{margin:0}.bot-tabs{overflow-x:auto}.bot-tabs .tabs{min-width:520px}.song-actions{flex-direction:column}.bot-flow small{display:none}}
</style>
