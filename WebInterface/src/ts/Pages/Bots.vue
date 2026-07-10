<template>
  <div class="bots-page">
    <nav class="flow-nav" aria-label="Ruta de navegación">
      <router-link to="/"><b-icon icon="home-outline" size="is-small"></b-icon> Inicio</router-link><b-icon icon="chevron-right" size="is-small"></b-icon><span>Bots</span>
    </nav>
    <header class="page-header">
      <div><span class="page-kicker">Workspace</span><h1 class="title is-2">Tus bots</h1><p>Administrá conexiones, estados y configuraciones desde un solo lugar.</p></div>
      <div class="bot-counter"><b-icon icon="robot"></b-icon><strong>{{ botsFiltered.length }}</strong><span>visibles</span></div>
    </header>
    <section class="filter-panel">
	  <div class="workflow-hint"><span>1</span><div><strong>Elegí o creá un bot</strong><small>Después abrí su servidor, cargá música y ajustá su configuración.</small></div></div>
      <b-field label="Acciones">
        <div class="buttons">
          <b-button
            icon-left="plus"
            class="is-success"
            @click="modalCreateBot = true"
            >Crear bot</b-button
          >
          <b-button
            icon-left="flash"
            class="is-success"
            @click="modalQuickConnect = true"
            >Conexión rápida</b-button
          >
        </div>
      </b-field>

	  <b-field label="Buscar y filtrar">
        <b-field grouped group-multiline>
          <b-input
            v-model="showFilter"
			icon="magnify"
            placeholder="Nombre o servidor"
            expanded
          ></b-input>
          <b-field>
            <b-checkbox-button
              v-model="showState"
              native-value="Connected"
              type="is-success"
			  >Conectados</b-checkbox-button
            >
            <b-checkbox-button
              v-model="showState"
              native-value="Connecting"
              type="is-warning"
			  >Conectando</b-checkbox-button
            >
            <b-checkbox-button
              v-model="showState"
              native-value="Offline"
              type="is-danger"
			  >Desconectados</b-checkbox-button
            >
          </b-field>

		  <b-tooltip class="control" label="Limpiar filtros">
            <b-button class="control" type="is-info" @click="clearFilter">
              <b-icon icon="filter-remove-outline" />
            </b-button>
          </b-tooltip>
        </b-field>
      </b-field>
    </section>

    <b-table
      v-if="displayTiles"
      :data="botsFiltered"
      :hoverable="true"
      :paginated="true"
      :per-page="10"
	  :row-class="rowClass"
	  @click="openBot"
    >
      <b-table-column field="Id" label="ID" width="40" numeric v-slot="props">
        <b-icon
          v-if="props.row && props.row.Id === null"
          icon="cancel"
        ></b-icon>
        {{ props.row ? props.row.Id : "" }}
      </b-table-column>
	  <b-table-column field="Name" label="Bot" v-slot="props">
		<span v-if="props.row && props.row.Id != null" class="bot-name-link"><b-icon icon="robot" size="is-small"></b-icon><strong>{{ props.row.Name }}</strong></span>
		<span v-else>{{ props.row ? props.row.Name : "" }}</span>
	  </b-table-column>
      <b-table-column field="Server" label="Server" v-slot="props">
        <edi-text
          v-if="props.row"
          :text="props.row.Server"
          @onedit="editBotServer($event, props.row)"
          :editable="props.row.Name && props.row.Status == BotStatus.Offline"
        />
      </b-table-column>
      <b-table-column field="Status" label="Status" v-slot="props">
        <span
          v-if="props.row"
          :class="'tag ' + statusToColor(props.row.Status)"
          >{{ BotStatus[props.row.Status] }}</span
        >
      </b-table-column>
	  <b-table-column label="Acciones" width="240" v-slot="props">
		<div v-if="props.row" class="bot-actions" @click.stop>
          <b-tooltip
            v-if="props.row.Id == null"
            class="control"
            label="Start"
            :delay="helpDelay"
          >
            <b-button type="is-success" @click="startBot(props.row.Name)">
              <b-icon icon="play" />
            </b-button>
          </b-tooltip>
          <b-tooltip v-else class="control" label="Stop" :delay="helpDelay">
            <b-button type="is-danger" @click="stopBot($event, props.row.Id)">
              <b-icon icon="power" />
            </b-button>
          </b-tooltip>
          <b-tooltip
            class="control"
			label="Abrir panel del bot"
            :delay="helpDelay"
          >
            <b-button
              :disabled="props.row.Id == null"
              tag="router-link"
              :to="
                props.row.Id != null
                  ? { name: 'r_server', params: { id: props.row.Id } }
                  : { name: 'r_bots' }
              "
              type="is-info"
            >
              <b-icon icon="file-tree" />
            </b-button>
          </b-tooltip>
          <b-tooltip
            class="control"
			label="Configuración"
            :delay="helpDelay"
          >
            <b-button
              tag="router-link"
              :to="
                props.row.Id != null
                  ? { name: 'r_settings', params: { id: props.row.Id } }
                  : {
                      name: 'r_settings_offline',
                      params: { name: props.row.Name },
                    }
              "
              type="is-info"
            >
              <b-icon icon="cog" />
            </b-button>
          </b-tooltip>
          <b-dropdown
            class="control"
            aria-role="list"
            position="is-bottom-left"
          >
            <button class="button is-primary" slot="trigger">
              <b-icon icon="dots-horizontal" />
            </button>

            <b-dropdown-item
              v-if="props.row.Name"
              @click="askDeleteBot(props.row.Name)"
            >
              <b-icon icon="delete" type="is-danger" />
              <span>Delete</span>
            </b-dropdown-item>
          </b-dropdown>
        </div>
      </b-table-column>
	  <template slot="empty"><div class="empty-state"><b-icon icon="robot-confused-outline" size="is-large"></b-icon><strong>No encontramos bots</strong><span>Creá uno nuevo o limpiá los filtros para continuar.</span></div></template>
    </b-table>

    <b-modal :active.sync="modalQuickConnect">
      <QuickConnectModal @callback="connectBot" />
    </b-modal>
    <b-modal :active.sync="modalDeleteBot">
      <DeleteBotModal @callback="deleteBot" :botName="interactBotName" />
    </b-modal>
    <b-modal :active.sync="modalCreateBot">
      <CreateBotModal @callback="createBot" />
    </b-modal>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { bot, cmd } from "../Api";
import { CmdBotInfo } from "../ApiObjects";
import { BotStatus } from "../Model/BotStatus";
import { Timer } from "../Timer";
import { Util } from "../Util";
import QuickConnectModal from "../Modals/QuickConnectModal.vue";
import { QuickConnectData } from "../ApiObjects";
import DeleteBotModal from "../Modals/DeleteBotModal.vue";
import CreateBotModal from "../Modals/CreateBotModal.vue";
import EditableText from "../Components/EditableText.vue";

export default Vue.extend({
  data() {
    return {
      BotStatus,
      helpDelay: 500,

      ticker: undefined! as Timer,
      bots: [] as CmdBotInfo[],
      hasConnectingBots: false,

      displayTiles: true,
      showState: ["Connected", "Connecting", "Offline"],
      showFilter: "",
      showLayout: "Table",

      interactBotName: "",
      modalQuickConnect: false,
      modalDeleteBot: false,
      modalCreateBot: false,
    };
  },
  computed: {
    botsFiltered(): CmdBotInfo[] {
      if (!this.bots || !this.bots.length) {
        return [];
      }

      return this.bots.filter((item) => {
        if (!item) return false;
        const connected = this.showState.indexOf("Connected") != -1;
        const connecting = this.showState.indexOf("Connecting") != -1;
        const offline = this.showState.indexOf("Offline") != -1;

        // Check if name or server matches the filter
        const nameMatch =
          item.Name == null || item.Name.indexOf(this.showFilter) >= 0;
        const serverMatch =
          item.Server == null || item.Server.indexOf(this.showFilter) >= 0;
        const textFilterMatch = nameMatch || serverMatch;

        // Check if status matches the selected states
        const statusMatch =
          (offline && item.Status == BotStatus.Offline) ||
          (connecting && item.Status == BotStatus.Connecting) ||
          (connected && item.Status == BotStatus.Connected);

        return textFilterMatch && statusMatch;
      });
    },
  },
  async created() {
    this.ticker = new Timer(async () => {
      if (!this.hasConnectingBots) {
        this.ticker.stop();
        return;
      }
      await this.refresh();
      if (!this.hasConnectingBots) this.ticker.stop();
    }, 1000);

    await this.refresh();
  },
  destroyed() {
    this.ticker.stop();
  },
  methods: {
	openBot(row: CmdBotInfo) {
	  if (row && row.Id != null) this.$router.push({ name: "r_server", params: { id: row.Id.toString() } });
	},
	rowClass(row: CmdBotInfo) {
	  return row && row.Id != null ? "is-clickable-bot" : "";
	},
    async refresh() {
      const res = await cmd<CmdBotInfo[]>("bot", "list").get();

		if (!Util.check(this, res, "Error getting bot list")) {
		  return;
      }

      this.hasConnectingBots = false;
      for (const botInfo of res) {
        if (botInfo.Status === BotStatus.Connecting) {
          this.hasConnectingBots = true;
          break;
        }
      }

      this.bots = res;
      if (this.hasConnectingBots) this.ticker.start();
    },
    clearFilter() {
      this.showState = ["Connected", "Connecting", "Offline"];
      this.showFilter = "";
    },
    statusToColor(status: BotStatus) {
      if (status == BotStatus.Connected) return "is-success";
      else if (status == BotStatus.Connecting) return "is-warning";
      else if (status == BotStatus.Offline) return "is-danger";
      else return "";
    },
	  async connectBot(data: QuickConnectData) {
		const res = await cmd<CmdBotInfo>(
			"bot", "connect", "to",
			data.address,
			data.password || "",
			data.channel || "",
			data.channelPassword || ""
		).get();
		if (!Util.check(this, res, "Error connecting bot")) {
        return;
      }
		this.modalQuickConnect = false;
		await this.refresh();
		if (res.Id != null) this.$router.push({ name: "r_server", params: { id: res.Id.toString() } });
    },
    askDeleteBot(name: string) {
      this.interactBotName = name;
      this.modalDeleteBot = true;
    },
    async deleteBot(name: string) {
      const res = await cmd<void>("settings", "delete", name).get();
      if (!Util.check(this, res, "Error deleting bot")) {
        return;
      }
      this.modalDeleteBot = false;
      await this.refresh();
    },
    async startBot(name: string) {
      const res = await cmd<CmdBotInfo>(
        "bot",
        "connect",
        "template",
        name
      ).get();
      if (!Util.check(this, res, "Error starting bot")) {
        return;
      }
      await this.refresh();
    },
    async stopBot(self: MouseEvent, id: number) {
      const res = await bot(cmd<void>("bot", "disconnect"), id).get();
      if (!Util.check(this, res, "Error stopping bot")) {
        return;
      }
      await this.refresh();
    },
    async createBot(name: string) {
      const res = await cmd<void>("settings", "create", name).get();
      if (!Util.check(this, res, "Error creating bot")) {
        return;
      }
      this.modalCreateBot = false;
      await this.refresh();
    },
    async editBotServer(server: string, bot: CmdBotInfo) {
		if (!bot.Name) {
		  this.$buefy.toast.open({ message: "Este bot no tiene una configuración editable", type: "is-warning" });
		  return;
      }

      const res = await cmd<void>(
        "settings",
        "bot",
        "set",
        bot.Name,
        "connect.address",
        server
      ).get();

      if (!Util.check(this, res, "Error setting server")) {
        return;
      }

      await this.refresh();
    },
  },
  components: {
    CreateBotModal,
    DeleteBotModal,
    QuickConnectModal,
    EditableText,
  },
});
</script>

<style lang="less">
.flow-nav{display:flex;align-items:center;gap:.4rem;margin-bottom:1.4rem;color:var(--muted);font-size:.82rem}.flow-nav a{display:flex;align-items:center;gap:.35rem;color:var(--muted)}.flow-nav a:hover{color:var(--brand)}.page-header{display:flex;align-items:flex-end;justify-content:space-between;gap:2rem;margin-bottom:2rem}.page-header .title{margin:.3rem 0!important;letter-spacing:-.04em}.page-header p{color:var(--muted)}.page-kicker{color:var(--brand);font-size:.72rem;font-weight:800;letter-spacing:.12em;text-transform:uppercase}.bot-counter{display:flex;align-items:center;gap:.55rem;padding:.75rem 1rem;background:var(--surface);border:1px solid var(--line);border-radius:14px;color:var(--brand)}.bot-counter strong{font-size:1.2rem}.bot-counter span{color:var(--muted);font-size:.8rem}.filter-panel{padding:1.35rem;margin-bottom:1.5rem;background:var(--surface);border:1px solid var(--line);border-radius:18px;box-shadow:var(--shadow)}.workflow-hint{display:flex;align-items:center;gap:.8rem;padding:.75rem 1rem;margin-bottom:1.2rem;border-radius:13px;background:rgba(109,93,252,.08)}.workflow-hint>span{display:grid;place-items:center;width:28px;height:28px;flex:none;border-radius:50%;background:var(--brand);color:#fff;font-weight:800}.workflow-hint div{display:flex;flex-direction:column}.workflow-hint small{color:var(--muted)}.filter-panel>.field:last-child{margin-bottom:0}.bots-page .table-wrapper{animation:rise .45s .08s ease both}.bot-actions{display:flex;align-items:center;gap:.5rem;white-space:nowrap}.bot-actions .control{margin:0!important}.bot-name-link{display:inline-flex;align-items:center;gap:.5rem;color:var(--text)}.bot-name-link:hover{color:var(--brand)}.empty-state{display:flex;flex-direction:column;align-items:center;gap:.5rem;padding:3rem;color:var(--muted)}.empty-state strong{color:var(--text);font-size:1.05rem}@media(max-width:768px){.page-header{align-items:flex-start}.bot-counter span{display:none}.filter-panel{padding:1rem}.filter-panel .field.is-grouped{display:block}.filter-panel .field.is-grouped>.control{margin-bottom:.65rem}.workflow-hint small{display:none}.bot-actions{min-width:230px}}
</style>
