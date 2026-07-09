<template>
	<div class="overview-page pure-g">
		<header class="overview-header"><div><span>Live telemetry</span><h1 class="title is-2">Estado del sistema</h1><p>Información y rendimiento de tu instancia en tiempo real.</p></div><div class="live-pill"><i></i> En vivo</div></header>
		<div class="tile is-ancestor">
			<div class="tile is-vertical">
				<div class="tile is-parent">
					<div class="tile is-child notification stat-card about-card">
						<span class="card-label"><b-icon icon="information-outline"></b-icon> Información</span>
						<div class="formcontent">
							<div class="formdatablock">
								<div>Version:</div>
								<div>{{aboutData.Version}}</div>
							</div>
							<div class="formdatablock">
								<div>Branch:</div>
								<div>{{aboutData.Branch}}</div>
							</div>
							<div class="formdatablock">
								<div>CommitHash:</div>
								<div>{{aboutData.CommitSha}}</div>
							</div>
							<br />
							<div class="formdatablock">
								<div>Uptime:</div>
								<div>{{aboutUptime}}</div>
							</div>
						</div>
					</div>
				</div>
				<div class="tile is-parent">
					<div class="tile is-child">
						<span class="title"></span>
					</div>
				</div>
			</div>
			<div class="tile is-vertical">
				<div class="tile is-parent">
					<div class="tile is-child notification stat-card">
						<span class="card-label"><b-icon icon="chip"></b-icon> CPU</span>
						<div id="data_cpugraph" style="position: relative;height: 10em;width: 100%;"></div>
					</div>
				</div>
				<div class="tile is-parent">
					<div class="tile is-child notification stat-card">
						<span class="card-label"><b-icon icon="memory"></b-icon> Memoria</span>
						<div id="data_memgraph" style="position: relative;height: 10em;width: 100%;"></div>
					</div>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from "vue";
import { cmd } from "../Api";
import { Graph, GraphOptions } from "../Graph";
import { Timer } from "../Timer";
import { Util } from "../Util";

const graphLen = 60;

export default Vue.extend({
	data() {
		return {
			ticker: undefined! as Timer,
			cpuGraphOptions: {
				color: "red",
				max: Graph.plusNPerc,
				offset: 0,
				scale: Graph.cpuTrim
			} as GraphOptions,
			memGraphOptions: {
				color: "blue",
				max: Graph.plusNPerc,
				offset: 0,
				scale: Graph.memTrim
			} as GraphOptions,

			showAbout: true,
			aboutData: {
				Version: "",
				Branch: "",
				CommitSha: ""
			},
			aboutUptime: ""
		};
	},
	async created() {
		this.ticker = new Timer(async () => await this.refresh(), 1000);
		this.ticker.start();

		const res = await cmd<{
			Version: string;
			Branch: string;
			CommitSha: string;
		}>("version").get();

		if (!Util.check(this, res, "Failed to get system information")) return;

		this.aboutData = res;
	},
	destroyed() {
		this.ticker.stop();
	},
	methods: {
		async refresh() {
			const res = await cmd<{
				memory: number[];
				cpu: number[];
				starttime: string;
			}>("system", "info").get();

			if (!Util.check(this, res, "Failed to get system information")) {
				this.ticker.stop();
				return;
			}

			if (!this.ticker.isRunning) {
				this.ticker.start();
			}

			res.cpu = this.padArray(res.cpu, graphLen, 0);
			Graph.buildPath(
				res.cpu,
				Util.getElementByIdSafe("data_cpugraph"),
				this.cpuGraphOptions
			);

			res.memory = this.padArray(res.memory, graphLen, 0);
			Graph.buildPath(
				res.memory,
				Util.getElementByIdSafe("data_memgraph"),
				this.memGraphOptions
			);

			this.aboutUptime = Util.formatSecondsToTime(
				(Date.now() - (new Date(res.starttime) as any)) / 1000
			);
		},
		padArray<T>(arr: T[], count: number, val: T): T[] {
			if (arr.length < count) {
				return Array<T>(count - arr.length)
					.fill(val)
					.concat(arr);
			}
			return arr;
		}
	}
});
</script>

<style lang="less">
.overview-header{display:flex;align-items:flex-end;justify-content:space-between;margin-bottom:1.5rem}.overview-header span{color:var(--brand);font-size:.72rem;font-weight:800;letter-spacing:.12em;text-transform:uppercase}.overview-header .title{margin:.35rem 0!important;letter-spacing:-.04em}.overview-header p{color:var(--muted)}.live-pill{display:flex;align-items:center;gap:.55rem;padding:.65rem .9rem;border:1px solid rgba(16,185,129,.2);border-radius:999px;background:rgba(16,185,129,.1);color:#10b981;font-size:.78rem;font-weight:800}.live-pill i{width:7px;height:7px;border-radius:50%;background:#10b981;box-shadow:0 0 0 5px rgba(16,185,129,.12);animation:pulse 2s infinite}.stat-card{min-height:230px;padding:1.5rem!important}.card-label{display:flex;align-items:center;gap:.5rem;margin-bottom:1.25rem;color:var(--muted);font-size:.78rem;font-weight:800;letter-spacing:.08em;text-transform:uppercase}.about-card .formdatablock{justify-content:space-between;border-bottom:1px solid var(--line);padding:.38rem 0}.about-card .formdatablock>div:last-child{font-family:monospace;color:var(--brand);word-break:break-all}.overview-page .tile.is-parent{padding:.5rem}@media(max-width:768px){.overview-header{align-items:flex-start;gap:1rem}.overview-header p{display:none}.stat-card{min-height:200px}}
</style>
