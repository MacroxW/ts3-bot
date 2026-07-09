<template>
	<div class="permissions-page container">
		<!-- Header -->
		<div class="permissions-header">
			<div>
				<p class="eyebrow"><b-icon icon="shield-lock" size="is-small"></b-icon> Administración</p>
				<h1 class="title is-2">Permisos del Sistema</h1>
				<p class="subtitle is-6">Modificá y recargá los privilegios de Dixibot de forma dinámica.</p>
			</div>
			<div class="header-actions">
				<b-button
					type="is-success"
					icon-left="content-save"
					:loading="saving"
					@click="savePermissions"
				>
					Guardar Cambios
				</b-button>
				<b-button
					type="is-light"
					icon-left="reload"
					:loading="loading || reloading"
					@click="loadPermissions"
				>
					Recargar
				</b-button>
			</div>
		</div>

		<!-- Main Panel -->
		<div class="permissions-layout">
			<!-- Left Column: TOML Editor -->
			<div class="editor-pane">
				<div class="pane-header">
					<h3><b-icon icon="code-braces" size="is-small"></b-icon> rights.toml</h3>
					<span class="status-indicator" :class="{'is-dirty': isDirty}">
						{{ isDirty ? 'Cambios sin guardar' : 'Guardado' }}
					</span>
				</div>
				<div class="textarea-wrapper">
					<textarea
						ref="editor"
						v-model="tomlContent"
						class="toml-textarea"
						placeholder="# Escribí tu configuración de permisos acá..."
						spellcheck="false"
						@keydown.tab.prevent="insertTab"
					></textarea>
				</div>
			</div>

			<!-- Right Column: Helper Sidebar (Tabs: Commands, Visual Rules) -->
			<div class="sidebar-pane">
				<b-tabs v-model="activeTab" class="sidebar-tabs" expanded>
					<!-- Tab 1: Visual Rules Analyzer -->
					<b-tab-item label="Visualizador de Reglas" icon="eye">
						<div class="rules-analyzer">
							<p class="pane-desc">Resumen visual de las reglas configuradas en el editor en tiempo real.</p>
							
							<div v-if="parsedRules.length === 0" class="empty-rules">
								<b-icon icon="alert-circle-outline" size="is-medium"></b-icon>
								<p>No se encontraron reglas en el archivo (o sólo hay permisos globales).</p>
							</div>

							<div v-else class="rules-list">
								<div v-for="(rule, idx) in parsedRules" :key="idx" class="visual-rule-card">
									<div class="rule-card-header">
										<h4>Regla #{{ idx + 1 }}</h4>
									</div>
									<div class="rule-matchers">
										<div class="matcher-group" v-if="rule.groups.length > 0">
											<span class="label-matcher">Grupos TS3:</span>
											<span v-for="g in rule.groups" :key="g" class="tag is-info is-light">{{ g }}</span>
										</div>
										<div class="matcher-group" v-if="rule.users.length > 0">
											<span class="label-matcher">UIDs Usuario:</span>
											<span v-for="u in rule.users" :key="u" class="tag is-success is-light" :title="u">{{ u.substring(0, 10) }}...</span>
										</div>
										<div class="matcher-group" v-if="rule.ips.length > 0">
											<span class="label-matcher">IPs:</span>
											<span v-for="ip in rule.ips" :key="ip" class="tag is-warning is-light">{{ ip }}</span>
										</div>
										<div class="matcher-group v-else-empty" v-if="rule.groups.length === 0 && rule.users.length === 0 && rule.ips.length === 0">
											<span class="tag is-danger is-light">Sin matchers (aplica a todos)</span>
										</div>
									</div>
									<div class="rule-permissions">
										<div v-if="rule.allowed.length > 0">
											<p class="perm-title has-text-success"><b-icon icon="check" size="is-small"></b-icon> Permitido (+)</p>
											<div class="tags">
												<span v-for="p in rule.allowed" :key="p" class="tag is-success is-light">{{ p }}</span>
											</div>
										</div>
										<div v-if="rule.denied.length > 0">
											<p class="perm-title has-text-danger"><b-icon icon="close" size="is-small"></b-icon> Denegado (-)</p>
											<div class="tags">
												<span v-for="p in rule.denied" :key="p" class="tag is-danger is-light">{{ p }}</span>
											</div>
										</div>
									</div>
								</div>
							</div>
						</div>
					</b-tab-item>

					<!-- Tab 2: Commands Dictionary -->
					<b-tab-item label="Diccionario de Comandos" icon="book-search">
						<div class="commands-dictionary">
							<b-field>
								<b-input
									v-model="searchQuery"
									placeholder="Buscar comando (ej. play, volume)"
									type="search"
									icon="magnify"
									clearable
								></b-input>
							</b-field>
							
							<div class="commands-scroll">
								<div
									v-for="cmd in filteredCommands"
									:key="cmd"
									class="command-item"
									@click="insertCommand(cmd)"
								>
									<span class="cmd-name">{{ cmd }}</span>
									<span class="cmd-hint">Click para insertar</span>
								</div>
								<div v-if="filteredCommands.length === 0" class="no-commands">
									No se encontraron comandos.
								</div>
							</div>
						</div>
					</b-tab-item>
				</b-tabs>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from "vue";
import { Get } from "../Api";
import { Util } from "../Util";

interface ParsedRule {
	groups: string[];
	users: string[];
	ips: string[];
	allowed: string[];
	denied: string[];
}

export default Vue.extend({
	data() {
		return {
			tomlContent: "",
			originalTomlContent: "",
			commandsList: [] as string[],
			searchQuery: "",
			loading: false,
			saving: false,
			reloading: false,
			activeTab: 0
		};
	},
	computed: {
		isDirty(): boolean {
			return this.tomlContent !== this.originalTomlContent;
		},
		filteredCommands(): string[] {
			const query = this.searchQuery.toLowerCase().trim();
			if (!query) return this.commandsList;
			return this.commandsList.filter(cmd => cmd.toLowerCase().includes(query));
		},
		parsedRules(): ParsedRule[] {
			return this.parseRules(this.tomlContent);
		}
	},
	created() {
		this.loadPermissions();
		this.loadCommands();
	},
	methods: {
		async loadPermissions() {
			this.loading = true;
			try {
				const auth = Get.AuthData;
				const headers = new Headers();
				if (!auth.IsAnonymous) {
					headers.append("Authorization", auth.getBasic());
				}
				const response = await fetch(Get.Endpoint + "/rights/get", {
					method: "GET",
					headers
				});
				if (!response.ok) {
					throw new Error("No se pudo obtener el archivo de permisos.");
				}
				const data = await response.json();
				this.tomlContent = data.Value || "";
				this.originalTomlContent = this.tomlContent;
			} catch (err: any) {
				this.$buefy.toast.open({
					message: `Error al cargar permisos: ${err.message || err}`,
					type: "is-danger",
					duration: 5000
				});
			} finally {
				this.loading = false;
			}
		},
		async loadCommands() {
			try {
				const auth = Get.AuthData;
				const headers = new Headers();
				if (!auth.IsAnonymous) {
					headers.append("Authorization", auth.getBasic());
				}
				const response = await fetch(Get.Endpoint + "/rights/commands", {
					method: "GET",
					headers
				});
				if (response.ok) {
					const data = await response.json();
					if (Array.isArray(data)) {
						this.commandsList = data;
					}
				}
			} catch (e) {
				console.error("Failed to load commands list", e);
			}
		},
		async savePermissions() {
			if (!this.tomlContent.trim()) {
				this.$buefy.toast.open({
					message: "El archivo de permisos no puede estar vacío.",
					type: "is-warning"
				});
				return;
			}
			this.saving = true;
			try {
				const auth = Get.AuthData;
				const headers = new Headers({
					"Content-Type": "application/json"
				});
				if (!auth.IsAnonymous) {
					headers.append("Authorization", auth.getBasic());
				}
				const response = await fetch(Get.Endpoint + "/rights/set", {
					method: "POST",
					headers,
					body: JSON.stringify({ toml: this.tomlContent })
				});

				const result = await response.json();
				if (!response.ok) {
					const errorMsg = result.Message || "Error desconocido";
					throw new Error(errorMsg);
				}

				this.$buefy.toast.open({
					message: "Permisos guardados y aplicados correctamente.",
					type: "is-success"
				});
				this.originalTomlContent = this.tomlContent;
			} catch (err: any) {
				this.$buefy.toast.open({
					message: `Error al guardar permisos: ${err.message || err}`,
					type: "is-danger",
					duration: 6000
				});
			} finally {
				this.saving = false;
			}
		},
		insertTab() {
			const textarea = this.$refs.editor as HTMLTextAreaElement;
			const start = textarea.selectionStart;
			const end = textarea.selectionEnd;
			this.tomlContent = this.tomlContent.substring(0, start) + "\t" + this.tomlContent.substring(end);
			this.$nextTick(() => {
				textarea.selectionStart = textarea.selectionEnd = start + 1;
			});
		},
		insertCommand(cmd: string) {
			const textarea = this.$refs.editor as HTMLTextAreaElement;
			const start = textarea.selectionStart;
			const end = textarea.selectionEnd;
			const toInsert = `"${cmd}"`;
			this.tomlContent = this.tomlContent.substring(0, start) + toInsert + this.tomlContent.substring(end);
			this.$nextTick(() => {
				textarea.focus();
				textarea.selectionStart = textarea.selectionEnd = start + toInsert.length;
			});
			this.$buefy.toast.open({
				message: `Insertado: ${cmd}`,
				type: "is-info",
				duration: 1500
			});
		},
		parseRules(toml: string): ParsedRule[] {
			const rules: ParsedRule[] = [];
			const lines = toml.split("\n");
			let currentRule: ParsedRule | null = null;
			let inGlobal = true;

			for (let line of lines) {
				line = line.trim();
				if (line.startsWith("#") || line === "") continue;

				if (line.startsWith("[[rule]]")) {
					inGlobal = false;
					if (currentRule) rules.push(currentRule);
					currentRule = {
						groups: [],
						users: [],
						ips: [],
						allowed: [],
						denied: []
					};
					continue;
				}

				if (line.includes("=")) {
					const idx = line.indexOf("=");
					const key = line.substring(0, idx).trim();
					const val = line.substring(idx + 1).trim();

					if (inGlobal) continue;
					if (!currentRule) continue;

					if (key === "groupid") {
						currentRule.groups = this.parseTomlArray(val);
					} else if (key === "useruid") {
						currentRule.users = this.parseTomlArray(val);
					} else if (key === "ip") {
						currentRule.ips = this.parseTomlArray(val);
					} else if (key === '"+"') {
						currentRule.allowed = this.parseTomlArrayOrString(val);
					} else if (key === '"-"') {
						currentRule.denied = this.parseTomlArrayOrString(val);
					}
				}
			}
			if (currentRule) rules.push(currentRule);
			return rules;
		},
		parseTomlArray(val: string): string[] {
			// Parses array like [ "123", "456" ] or [ 12, 34 ] or []
			const trimmed = val.trim();
			if (!trimmed.startsWith("[") || !trimmed.endsWith("]")) return [];
			const content = trimmed.substring(1, trimmed.length - 1).trim();
			if (!content) return [];
			return content.split(",").map(s => {
				const item = s.trim();
				if (item.startsWith('"') && item.endsWith('"')) {
					return item.substring(1, item.length - 1);
				}
				return item;
			}).filter(s => s.length > 0);
		},
		parseTomlArrayOrString(val: string): string[] {
			const trimmed = val.trim();
			if (trimmed.startsWith("[")) {
				return this.parseTomlArray(trimmed);
			}
			if (trimmed.startsWith('"') && trimmed.endsWith('"')) {
				return [trimmed.substring(1, trimmed.length - 1)];
			}
			return [trimmed];
		}
	}
});
</script>

<style lang="less">
.permissions-page {
	padding-top: 1.5rem;
	padding-bottom: 3rem;
	max-width: 1240px;
	margin: 0 auto;
}

.permissions-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 2rem;
	border-bottom: 1px solid var(--line);
	padding-bottom: 1.5rem;

	.title {
		margin-bottom: 0.35rem !important;
		color: var(--text);
		font-weight: 800;
		letter-spacing: -0.02em;
	}

	.subtitle {
		color: var(--muted);
	}

	.header-actions {
		display: flex;
		gap: 0.75rem;
	}
}

.permissions-layout {
	display: grid;
	grid-template-columns: 1.2fr .8fr;
	gap: 1.5rem;
	align-items: start;
}

.editor-pane {
	background: var(--surface);
	border: 1px solid var(--line);
	border-radius: 20px;
	overflow: hidden;
	box-shadow: var(--shadow);
	display: flex;
	flex-direction: column;

	.pane-header {
		padding: 1rem 1.25rem;
		background: rgba(0, 0, 0, 0.08);
		border-bottom: 1px solid var(--line);
		display: flex;
		justify-content: space-between;
		align-items: center;

		h3 {
			font-weight: 700;
			color: var(--text);
			display: flex;
			align-items: center;
			gap: 0.5rem;
		}

		.status-indicator {
			font-size: 0.75rem;
			font-weight: 700;
			text-transform: uppercase;
			letter-spacing: 0.05em;
			color: var(--muted);

			&.is-dirty {
				color: var(--brand);
			}
		}
	}

	.textarea-wrapper {
		position: relative;
	}

	.toml-textarea {
		width: 100%;
		height: 60vh;
		min-height: 500px;
		background: transparent;
		border: none;
		color: var(--text);
		font-family: "Fira Code", Consolas, Monaco, "Andale Mono", "Ubuntu Mono", monospace;
		font-size: 0.95rem;
		line-height: 1.6;
		padding: 1.25rem;
		resize: vertical;
		outline: none;

		&:focus {
			box-shadow: none;
		}
	}
}

.sidebar-pane {
	background: var(--surface);
	border: 1px solid var(--line);
	border-radius: 20px;
	overflow: hidden;
	box-shadow: var(--shadow);

	.sidebar-tabs {
		.tab-content {
			padding: 1.25rem;
			max-height: 65vh;
			overflow-y: auto;
		}
	}

	.pane-desc {
		font-size: 0.85rem;
		color: var(--muted);
		margin-bottom: 1.25rem;
		line-height: 1.5;
	}
}

.commands-dictionary {
	.commands-scroll {
		display: flex;
		flex-direction: column;
		gap: 0.4rem;
		margin-top: 0.75rem;
	}

	.command-item {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 0.65rem 0.85rem;
		background: rgba(109, 93, 252, 0.04);
		border: 1px solid var(--line);
		border-radius: 10px;
		cursor: pointer;
		transition: all 0.2s ease;

		&:hover {
			border-color: var(--brand);
			background: rgba(109, 93, 252, 0.08);

			.cmd-name {
				color: var(--brand);
			}

			.cmd-hint {
				opacity: 1;
				transform: translateX(0);
			}
		}

		.cmd-name {
			font-family: monospace;
			font-weight: 700;
			color: var(--text);
			font-size: 0.85rem;
		}

		.cmd-hint {
			font-size: 0.7rem;
			color: var(--brand);
			opacity: 0;
			transform: translateX(-5px);
			transition: all 0.2s ease;
			font-weight: 650;
		}
	}

	.no-commands {
		text-align: center;
		color: var(--muted);
		padding: 2rem 0;
	}
}

.rules-analyzer {
	.empty-rules {
		text-align: center;
		color: var(--muted);
		padding: 3rem 0;
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 0.5rem;
	}

	.rules-list {
		display: flex;
		flex-direction: column;
		gap: 1rem;
	}

	.visual-rule-card {
		background: rgba(255, 255, 255, 0.02);
		border: 1px solid var(--line);
		border-radius: 12px;
		padding: 1rem;
		display: flex;
		flex-direction: column;
		gap: 0.75rem;

		.rule-card-header {
			border-bottom: 1px solid var(--line);
			padding-bottom: 0.4rem;

			h4 {
				font-weight: 750;
				font-size: 0.9rem;
				color: var(--brand-2);
			}
		}

		.rule-matchers {
			display: flex;
			flex-direction: column;
			gap: 0.5rem;

			.matcher-group {
				display: flex;
				flex-wrap: wrap;
				align-items: center;
				gap: 0.35rem;

				.label-matcher {
					font-size: 0.75rem;
					font-weight: 700;
					color: var(--muted);
					min-width: 90px;
				}
			}
		}

		.rule-permissions {
			display: flex;
			flex-direction: column;
			gap: 0.5rem;
			border-top: 1px dashed var(--line);
			padding-top: 0.65rem;

			.perm-title {
				font-size: 0.75rem;
				font-weight: 800;
				display: flex;
				align-items: center;
				gap: 0.25rem;
				margin-bottom: 0.25rem;
			}

			.tags {
				margin-bottom: 0 !important;

				.tag {
					font-family: monospace;
					font-size: 0.75rem;
				}
			}
		}
	}
}

@media (max-width: 1023px) {
	.permissions-layout {
		grid-template-columns: 1fr;
	}
}
</style>
