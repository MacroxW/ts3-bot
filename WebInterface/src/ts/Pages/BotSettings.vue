<template>
	<section class="settings-page">
		<div class="settings-toolbar">
			<b-input icon="magnify" v-model="filter.text" placeholder="Buscar configuración..." expanded />

			<b-field class="level-switch">
				<b-radio-button
					v-model="filter.level"
					:native-value="SettLevel.Beginner"
					type="is-success"
				>Básico</b-radio-button>
				<b-radio-button
					v-model="filter.level"
					:native-value="SettLevel.Advanced"
					type="is-warning"
				>Avanzado</b-radio-button>
				<b-radio-button v-model="filter.level" :native-value="SettLevel.Expert" type="is-danger">Experto</b-radio-button>
			</b-field>
		</div>

		<settings-group label="General">
			<settings-field :filter="filter" path="run" label="Conectar cuando Dixibot inicia">
				<b-switch v-model="model.run" size="is-medium"></b-switch>
			</settings-field>
			<settings-field :filter="filter" path="generate_status_avatar" label="Load song cover as avatar">
				<b-switch v-model="model.generate_status_avatar" size="is-medium"></b-switch>
			</settings-field>
			<settings-field
				:filter="filter"
				path="set_status_description"
				label="Show song in bot description"
			>
				<b-switch v-model="model.set_status_description" size="is-medium"></b-switch>
			</settings-field>

			<settings-field :filter="filter" path="language" label="Bot Language">
				<b-select v-model="model.language" placeholder="Select your language">
					<option v-for="lang in Object.keys(Lang)" :key="lang" :value="lang">{{Lang[lang]}}</option>
				</b-select>
			</settings-field>
		</settings-group>

		<settings-group label="Connection">
			<settings-field :filter="filter" path="connect.name" label="Bot name" grouped>
				<b-input v-model="model.connect.name" minlength="3" maxlength="30" expanded required></b-input>
				<b-button v-if="online" class="control" @click="botRename(model.connect.name)">Apply to bot now</b-button>
			</settings-field>
			<settings-field :filter="filter" path="connect.address" label="Server address" grouped>
				<b-input v-model="model.connect.address" expanded required></b-input>
			</settings-field>

			<settings-field :filter="filter" path="connect.server_password" label="Server password">
				<settings-password :filter="filter" v-model="model.connect.server_password" />
			</settings-field>

			<settings-field :filter="filter" path="connect.channel" label="Canal predeterminado">
				<b-input v-model="model.connect.channel" placeholder="Ruta del canal o /ID, por ejemplo /5" expanded></b-input>
			</settings-field>

			<settings-field :filter="filter" path="connect.channel_password" label="Channel password">
				<settings-password :filter="filter" v-model="model.connect.channel_password" />
			</settings-field>

			<settings-field
				:filter="filter"
				path="connect.client_version"
				label="Emulated client version"
				advanced
			>
				<b-select v-model="model.connect.client_version" placeholder="Select version">
					<option
						v-for="ver in versions"
						:key="ver.build + ver.platform"
						:value="ver"
					>{{ver.build}} : {{ver.platform}}</option>
				</b-select>
			</settings-field>
		</settings-group>

		<settings-group label="Audio">
			<settings-field :filter="filter" label="Default volume" path="audio.volume.default" grouped>
				<div class="control is-expanded">
					<b-slider v-model="model.audio.volume.default" :min="0" :max="100" lazy></b-slider>
				</div>
				<b-button class="control" icon-left="volume-high" :loading="loadingCurrentVolume" @click="applyCurrentVolume">Usar volumen actual</b-button>
			</settings-field>

			<settings-field :filter="filter" label="New song volume" path="audio.volume" advanced>
				<b-slider v-model="bind_volume_reset" :min="0" :max="100" lazy></b-slider>
			</settings-field>

			<settings-field :filter="filter" label="Max user volume" path="audio.max_user_volume" advanced>
				<b-slider v-model="model.audio.max_user_volume" :min="0" :max="100" lazy></b-slider>
			</settings-field>

			<settings-field :filter="filter" label="Bitrate" path="audio.bitrate" grouped advanced>
				<b-field>
					<b-radio-button v-model="model.audio.bitrate" :native-value="16" type="is-danger">16</b-radio-button>
					<b-radio-button v-model="model.audio.bitrate" :native-value="24" type="is-danger">24</b-radio-button>
					<b-radio-button v-model="model.audio.bitrate" :native-value="32" type="is-warning">32</b-radio-button>
					<b-radio-button v-model="model.audio.bitrate" :native-value="48" type="is-warning">48</b-radio-button>
					<b-radio-button v-model="model.audio.bitrate" :native-value="64" type="is-success">64</b-radio-button>
					<b-radio-button v-model="model.audio.bitrate" :native-value="96" type="is-success">96 kbps</b-radio-button>
				</b-field>
				<b-field expanded>
					<b-slider v-model="model.audio.bitrate" :min="2" :max="128" :step="2" expanded></b-slider>
				</b-field>
			</settings-field>
		</settings-group>

		<settings-group label="Commands">
			<settings-field :filter="filter" label="Matcher" path="commands.matcher" expert>
				<b-select v-model="model.commands.matcher" placeholder="Select your matcher">
					<option value="ic3">IC3</option>
					<option value="exact">Exact</option>
					<option value="substring">Substring</option>
				</b-select>
			</settings-field>

			<settings-field
				:filter="filter"
				label="How the bot treats long messages"
				path="commands.long_message"
				advanced
			>
				<b-select
					v-model="model.commands.long_message"
					placeholder="Select how the bot treats long messages"
				>
					<option value="2">Drop (Message will not be sent)</option>
					<option value="0">Split (Message will be split up into multiple messages)</option>
				</b-select>
			</settings-field>
			<settings-field
				:filter="filter"
				label="In how many messages a message can be split max"
				path="commands.long_message_split_limit"
				advanced
			>
				<b-numberinput
					v-model="model.commands.long_message_split_limit"
					controls-position="compact"
					:disabled="model.commands.long_message == 2"
				/>
			</settings-field>
			<settings-field
				:filter="filter"
				label="Max command complexity"
				path="commands.command_complexity"
				expert
			>
				<b-numberinput v-model="model.commands.command_complexity" controls-position="compact" />
			</settings-field>
			<settings-field :filter="filter" label="Colored chat messages" path="commands.color" advanced>
				<b-switch v-model="model.commands.color" size="is-medium"></b-switch>
			</settings-field>
		</settings-group>
	</section>
</template>

<script lang="ts">
import Vue from "vue";
import SettingsField from "../Components/SettingsField.vue";
import SettingsGroup from "../Components/SettingsGroup.vue";
import SettingsPassword from "../Components/SettingsPassword.vue";
import { bot, cmd, Api } from "../Api";
import { IVersion } from "../ApiObjects";
import { Util } from "../Util";
import Lang from "../Model/Languge";
import { debounce } from "lodash-es";
import { SettLevel } from "../Model/SettingsLevel";

// missing:
// - channel password
// - server password
// - client version
// - identity

// - send_mode

// - events

// - reconnect

// - aliases

export default Vue.extend({
	props: {
		online: { type: Boolean, required: true }
	},
	data() {
		return {
			Lang,
			SettLevel,

			versions: [] as IVersion[],
			loadingCurrentVolume: false,
			originalBotName: "",
			filter: {
				text: "",
				level: Number(localStorage.filter_level ?? SettLevel.Beginner) as SettLevel,
			},
			model: {
				audio: {
					volume: {},
					bitrate: 0
				},
				connect: {
					server_password: {},
					channel_password: {}
				},
				commands: {}
			} as any
		};
	},
	async created() {
		const res = await this.requestModel();
		fetch(
			"https://raw.githubusercontent.com/ReSpeak/tsdeclarations/master/Versions.csv"
		)
			.then(v => v.text())
			.then(csv => {
				this.versions = csv
					.split(/\n/gm)
					.slice(1)
					.map(line => line.split(/,/g))
					.map(parts => ({
						build: parts[0],
						platform: parts[1],
						sign: parts[2]
					}))
					.filter(ver => {
						const buildM = ver.build.match(/\[Build: (\d+)\]/);
						if (buildM == null) return true;
						return Number(buildM[1]) > 1513163251; // > 3.1.7 required
					});
			});

		if (!Util.check(this, res, "Failed to retrieve settings")) return;

		this.model = res;
		this.originalBotName = this.model.connect.name;

		this.bindRecursive("", this.model);
	},
	watch: {
		"filter.level"(val: SettLevel): void {
			localStorage.filter_level = val;
		}
	},
	computed: {
		botId(): number | string {
			if (this.online) return Number(this.$route.params.id);
			else return this.$route.params.name;
		},
		bind_volume_reset: {
			get(): [number, number] {
				return [
					this.model.audio.volume.min,
					this.model.audio.volume.max
				];
			},
			set(value: [number, number]) {
				this.model.audio.volume.min = value[0];
				this.model.audio.volume.max = value[1];
			}
		}
	},
	methods: {
		async applyCurrentVolume() {
			if (!this.online) return;
			this.loadingCurrentVolume = true;
			try {
				const res = await bot(cmd<number>("volume"), this.botId).get();
				if (!Util.check(this, res, "No se pudo obtener el volumen actual")) return;
				this.model.audio.volume.default = Math.round(res);
			} finally {
				this.loadingCurrentVolume = false;
			}
		},
		requestModel(): Promise<Api<any>> {
			if (this.online)
				return bot(cmd<any>("settings", "get"), this.botId).get();
			else
				return cmd<any>(
					"settings",
					"bot",
					"get",
					this.botId.toString()
				).get();
		},
		sendValue(confVal: string, val: string | number | object) {
			if (typeof val === "object") val = JSON.stringify(val);
			if (this.online)
				return bot(
					cmd<void>("settings", "set", confVal, val),
					this.botId
				).get();
			else
				return cmd<void>(
					"settings",
					"bot",
					"set",
					this.botId.toString(),
					confVal,
					val
				).get();
		},
		isSettingsObjectGroup(path: string): boolean {
			return path.startsWith("connect.client_version");
		},
		bindRecursive(path: string, obj: any) {
			for (const childKey of Object.keys(obj)) {
				var child: any = obj[childKey];
				var childPath = (path ? path + "." : "") + childKey;
				if (
					typeof child === "object" &&
					!Array.isArray(child) &&
					!this.isSettingsObjectGroup(childPath)
				) {
					this.bindRecursive(childPath, child);
				} else {
					this.doWatch(childPath, child);
				}
			}
		},
		doWatch(confVal: string, child: any) {
			this.$watch(
				"model." + confVal,
				debounce(async function(val) {
					const res = await this.sendValue(confVal, val);
					if (!Util.check(this, res, "Failed to apply")) return;

					this.$buefy.toast.open({
						duration: 500,
						message: "Saved",
						type: "is-success"
					});
				}, ...this.getBounceDelay(typeof child))
			);
		},
		getBounceDelay(type: string): [number, object] {
			if (type === "string") {
				return [
					1000,
					{
						leading: false,
						trailing: true
					}
				];
			} else {
				return [
					1000,
					{
						leading: true,
						trailing: true,
						maxWait: 100
					}
				];
			}
		},
		async botRename(name: string) {
			if (name === this.originalBotName) {
				this.$buefy.toast.open({ message: "El bot ya utiliza ese nombre", type: "is-info" });
				return;
			}
			const res = await bot(
				cmd<void>("bot", "name", name),
				this.botId
			).get();

			if (!Util.check(this, res, "Failed to set name")) return;
			this.originalBotName = name;
		}
	},
	components: {
		SettingsField,
		SettingsGroup,
		SettingsPassword
	}
});
</script>

<style lang="less">
.settings-toolbar{display:grid;grid-template-columns:minmax(220px,1fr) auto;align-items:center;gap:.75rem;margin-bottom:1rem;padding:.8rem;border:1px solid var(--line);border-radius:16px;background:var(--surface)}.settings-toolbar .field{margin:0}.settings-toolbar>.control{width:100%}.level-switch .field{flex-wrap:nowrap}.settings-page>.card{margin-bottom:.85rem}.settings-page>.card:last-child{margin-bottom:0}.settings-page .b-slider{margin:.35rem .25rem}.settings-page .b-radio.radio.button{min-height:38px;height:38px;padding:.45rem .7rem;font-size:.78rem}.settings-page .select,.settings-page .select select{width:100%}@media(max-width:720px){.settings-toolbar{grid-template-columns:1fr}.level-switch>.field{display:grid;grid-template-columns:repeat(3,1fr)}.level-switch .b-radio.radio.button{width:100%;justify-content:center}.settings-page>.card{margin-bottom:.7rem}}
</style>
