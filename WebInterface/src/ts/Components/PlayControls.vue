<template>
	<section id="playcontrols" class="is-fixed-bottom">
		<div class="player-shell">
			<div class="track-meta">
				<div class="track-icon"><b-icon :icon="playing === PlayState.Playing ? 'waveform' : 'music-note'"></b-icon></div>
				<div><strong>{{ info.song ? info.song.Title : 'Sin reproducción' }}</strong><span>{{ info.song ? 'Reproduciendo en Dixibot' : 'Agregá una canción para comenzar' }}</span></div>
			</div>
			<div class="player-main">
				<div class="transport-controls">
					<b-button class="player-button secondary-action" @click="clickRepeat" title="Repetición">
						<b-icon :icon="repeat_icon" />
					</b-button>
					<b-button class="player-button secondary-action" @click="clickShuffle" title="Aleatorio">
						<b-icon :icon="shuffle_icon" />
					</b-button>
					<b-button class="player-button" @click="clickTrackPrev" title="Anterior">
						<b-icon icon="skip-previous" />
					</b-button>
					<b-button class="player-button play-button" @click="clickPlay" :disabled="!info.song" title="Reproducir o pausar">
						<b-icon class="is-medium" :icon="play_icon" />
					</b-button>
					<b-button class="player-button" @click="clickTrackNext" title="Siguiente">
						<b-icon icon="skip-next" />
					</b-button>
				</div>
				<div class="timeline">
					<span>{{song_position_human}}</span>
					<div class="timeline-slider">
						<b-slider
							@change="setPosition"
							v-model="song_pos_safe"
							:tooltip="false"
							:max="song_length_safe"
							:disabled="!info.song"
							rounded
							lazy
						></b-slider>
					</div>
					<span>{{song_length_human}}</span>
				</div>
			</div>
			<div class="volume-controls">
					<b-button class="player-button secondary-action" @click="clickVolume" title="Silenciar">
						<b-icon :icon="volume_icon" />
					</b-button>
					<div class="volume-slider"><b-slider v-model="info.volume" :min="0" :max="100" :tooltip="false"></b-slider></div>
					<span>{{Math.round(info.volume)}}%</span>
			</div>
		</div>
	</section>
</template>

<script lang="ts">
import Vue from "vue";
import { RepeatKind } from "../Model/RepeatKind";
import { Util } from "../Util";
import { bot, jmerge, cmd, ApiErr, all } from "../Api";
import { PlayState } from "../Model/PlayState";
import { CmdSong } from "../ApiObjects";
import { Timer } from "../Timer";
import { debounce } from "lodash-es";
import { BotInfoSync } from "../Model/BotInfoSync";

export default Vue.component("play-controls", {
	props: {
		botId: { type: Number, required: true },
		info: { type: Object as () => BotInfoSync, required: true }
	},
	async created() {
		this.playTick = new Timer(() => {
			if (!this.info.song) {
				this.playTick.stop();
				return;
			}
			if (this.info.song.Position < this.info.song.Length) {
				this.info.song.Position += 1;
			} else {
				this.playTick.stop();
				this.startEcho();
			}
		}, 1000);

		this.echoTick = new Timer(async () => {
			this.echoCounter += 1;
			if (
				this.echoCounter === 1 ||
				this.echoCounter === 3 ||
				this.echoCounter === 6
			) {
				await this.refresh();
			}
			if (this.echoCounter >= 6) {
				this.echoTick.stop();
			}
		}, 1000);

		this.$watch("info.song", this.updateTimers, { deep: true });
	},
	data() {
		return {
			RepeatKind,
			PlayState,

			volume_old: 0,
			muteToggleVolume: 0,

			echoCounter: 0,
			echoTick: undefined! as Timer,
			playTick: undefined! as Timer
		};
	},
	computed: {
		song_pos_safe: {
			get(): number {
				if (!this.info.song) return 0;
				return this.info.song.Position;
			},
			set(val: number) {
				if (this.info.song) this.info.song.Position = val;
			}
		},
		song_length_safe(): number {
			if (!this.info.song) return 0;
			return this.info.song.Length;
		},
		song_position_human(): string {
			if (!this.info.song) return "--:--";
			return Util.formatSecondsToTime(this.info.song.Position);
		},
		song_length_human(): string {
			if (!this.info.song) return "--:--";
			return Util.formatSecondsToTime(this.info.song.Length);
		},
		playing(): PlayState {
			if (!this.info.song) return PlayState.Off;
			else if (this.info.song.Paused) return PlayState.Paused;
			else return PlayState.Playing;
		},
		play_icon() {
			switch (this.playing) {
				case PlayState.Off:
					return "heart";
				case PlayState.Playing:
					return "pause";
				case PlayState.Paused:
					return "play";
				default:
					throw Error();
			}
		},
		repeat_icon() {
			switch (this.info.repeat) {
				case RepeatKind.Off:
					return "repeat-off";
				case RepeatKind.One:
					return "repeat-once";
				case RepeatKind.All:
					return "repeat";
				default:
					throw Error();
			}
		},
		shuffle_icon(): string {
			return this.info.shuffle ? "shuffle" : "shuffle-disabled";
		},
		volume_icon(): string {
			if (this.info.volume <= 0.001) return "volume-off";
			else if (this.info.volume <= 33) return "volume-low";
			else if (this.info.volume <= 66) return "volume-medium";
			else return "volume-high";
		},
		setVD(): Function {
			return debounce(this.setVolume, 500, {
				maxWait: 100
			});
		}
	},
	methods: {
		async clickRepeat() {
			const res = await bot(
				jmerge(
					cmd<void>(
						"repeat",
						RepeatKind[(this.info.repeat + 1) % 3].toLowerCase()
					),
					cmd<RepeatKind>("repeat")
				),
				this.botId
			).get();
			if (!Util.check(this, res, "Failed to apply repeat mode")) return;

			this.info.repeat = res[1];
		},
		async clickShuffle() {
			const res = await bot(
				jmerge(
					cmd<void>("random", !this.info.shuffle ? "on" : "off"),
					cmd<boolean>("random")
				),
				this.botId
			).get();
			if (!Util.check(this, res, "Failed to apply random mode")) return;

			this.info.shuffle = res[1];
		},
		async clickVolume() {
			if (this.muteToggleVolume !== 0 && this.info.volume === 0) {
				await this.setVolume(this.muteToggleVolume);
				this.muteToggleVolume = 0;
			} else {
				this.muteToggleVolume = this.info.volume;
				await this.setVolume(0);
			}
		},
		async setVolume(value: number) {
			const res = await bot(
				jmerge(
					cmd<void>("volume", value.toString()),
					cmd<number>("volume")
				),
				this.botId
			).get();
			if (!Util.check(this, res, "Failed to apply volume")) {
				this.info.volume = this.volume_old;
				return;
			}
			this.volume_old = this.info.volume;
		},
		async clickTrackNext() {
			const res = await bot(cmd<void>("next"), this.botId).get();
			if (!Util.check(this, res, "Failed to skip forward")) return;
			this.startEcho();
		},
		async clickTrackPrev() {
			const res = await bot(cmd<void>("previous"), this.botId).get();
			if (!Util.check(this, res, "Failed to skip forward")) return;
			this.startEcho();
		},
		async clickPlay() {
			let songRet: ApiErr | [void, CmdSong | null];
			switch (this.playing) {
				case PlayState.Off:
					return;

				case PlayState.Playing:
					songRet = await bot(
						jmerge(cmd<void>("pause"), cmd<CmdSong | null>("song")),
						this.botId
					).get();
					this.playTick.stop();
					break;

				case PlayState.Paused:
					songRet = await bot(
						jmerge(cmd<void>("play"), cmd<CmdSong | null>("song")),
						this.botId
					).get();
					this.playTick.start();
					break;

				default:
					throw new Error();
			}

			if (!Util.check(this, songRet)) return;

			this.info.song = songRet[1];
			this.startEcho();
		},
		async setPosition(value: number) {
			if (this.playing === PlayState.Off) return;

			const wasRunning = this.playTick.isRunning;
			this.playTick.stop();
			const targetSeconds = Math.floor(value);
			const res = await bot(
				cmd<void>("seek", targetSeconds.toString()),
				this.botId
			).get();

			if (!Util.check(this, res, "Failed to seek")) return;

			if (wasRunning) this.playTick.start();
			if (this.info.song) this.info.song.Position = targetSeconds;
		},
		startEcho() {
			this.echoCounter = 0;
			this.echoTick.start();
		},
		async refresh() {
			this.$emit("requestRefresh");
		}, 
		updateTimers() {
			if (this.playing == PlayState.Playing) this.playTick.start();
			else this.playTick.stop();
		}
	},
	watch: {
		"info.volume"(value: number) {
			this.setVD(value);
		}
	}
});
</script>

<style lang="less">
#playcontrols{left:0;right:0;bottom:0;z-index:30;padding:.75rem 1rem;background:linear-gradient(180deg,transparent,rgba(8,13,25,.18))}.player-shell{display:grid;grid-template-columns:minmax(170px,1fr) minmax(320px,2fr) minmax(170px,1fr);align-items:center;gap:1.25rem;max-width:1240px;min-height:76px;margin:auto;padding:.65rem 1rem;border:1px solid var(--line);border-radius:20px;background:var(--surface-solid);box-shadow:0 -12px 45px rgba(15,23,42,.18);backdrop-filter:blur(22px)}.track-meta{display:flex;align-items:center;gap:.75rem;min-width:0}.track-meta>div:last-child{display:flex;flex-direction:column;min-width:0}.track-meta strong{overflow:hidden;text-overflow:ellipsis;white-space:nowrap;color:var(--text);font-size:.88rem}.track-meta span{overflow:hidden;text-overflow:ellipsis;white-space:nowrap;color:var(--muted);font-size:.7rem}.track-icon{display:grid;place-items:center;width:42px;height:42px;flex:none;border-radius:12px;background:linear-gradient(135deg,var(--brand),var(--brand-2));color:#fff}.player-main{display:flex;flex-direction:column;gap:.25rem}.transport-controls{display:flex;align-items:center;justify-content:center;gap:.25rem}.player-button{min-width:36px!important;min-height:36px!important;width:36px;height:36px;padding:0!important;border:0!important;border-radius:50%!important;background:transparent!important;color:var(--text)!important;box-shadow:none!important}.player-button:hover{background:rgba(109,93,252,.1)!important;color:var(--brand)!important;transform:none!important}.player-button.secondary-action{color:var(--muted)!important}.play-button{width:44px;height:44px;background:linear-gradient(135deg,var(--brand),#8b5cf6)!important;color:#fff!important;box-shadow:0 8px 20px rgba(109,93,252,.28)!important}.play-button:hover{color:#fff!important;transform:scale(1.05)!important}.timeline{display:grid;grid-template-columns:42px 1fr 42px;align-items:center;gap:.6rem;color:var(--muted);font-size:.68rem}.timeline>span:last-child{text-align:right}.timeline-slider,.volume-slider{min-width:0}.timeline .b-slider,.volume-slider .b-slider{margin:0}.volume-controls{display:flex;align-items:center;justify-content:flex-end;gap:.5rem}.volume-slider{width:100px}.volume-controls>span{width:34px;color:var(--muted);font-size:.68rem;text-align:right}@media(max-width:900px){.player-shell{grid-template-columns:1fr auto}.player-main{grid-column:1/-1;grid-row:1}.track-meta{grid-row:2}.volume-controls{grid-row:2}.timeline{margin-top:.2rem}}@media(max-width:600px){#playcontrols{padding:.45rem}.player-shell{min-height:68px;padding:.5rem .65rem;border-radius:16px}.track-meta,.volume-controls{display:none}.player-main{grid-column:1/-1;grid-row:auto}.transport-controls{gap:.5rem}.timeline{grid-template-columns:34px 1fr 34px}.secondary-action{display:none!important}}
</style>
