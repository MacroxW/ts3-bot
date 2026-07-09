<template>
	<div class="permissions-page container">
		<!-- Header -->
		<div class="permissions-header">
			<div>
				<p class="eyebrow"><b-icon icon="shield-lock" size="is-small"></b-icon> Administración</p>
				<h1 class="title is-2">Permisos del Sistema</h1>
				<p class="subtitle is-6 font-muted">Gestioná los privilegios de Dixibot de forma visual y segura sin romper la sintaxis del archivo.</p>
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

		<!-- Tabs for Visual vs Code Mode -->
		<b-tabs v-model="activeTab" class="permissions-tabs" @input="onTabChange" expanded>
			<!-- Tab 1: Visual Manager -->
			<b-tab-item label="Gestor Visual" icon="tune-vertical">
				<div class="visual-manager-layout">
					<!-- Global Perms Card -->
					<div class="visual-rule-card card mb-5 global-perms-card">
						<header class="card-header">
							<p class="card-header-title">
								<b-icon icon="earth" class="mr-2"></b-icon>
								Permisos Globales (Aplica a Todos)
							</p>
						</header>
						<div class="card-content">
							<div class="columns">
								<div class="column is-6">
									<b-field label="Permitir (+) Globalmente" message="Comandos accesibles para cualquier usuario.">
										<b-taginput
											v-model="model.globalAllowed"
											:data="filteredAutocompleteCommands"
											autocomplete
											open-on-focus
											allow-new
											placeholder="Ej. cmd.play, cmd.help.*"
											icon="check-circle"
											type="is-success"
											@typing="getFilteredCommands"
										></b-taginput>
									</b-field>
								</div>
								<div class="column is-6">
									<b-field label="Denegar (-) Globalmente" message="Comandos bloqueados para todos excepto administradores.">
										<b-taginput
											v-model="model.globalDenied"
											:data="filteredAutocompleteCommands"
											autocomplete
											open-on-focus
											allow-new
											placeholder="Ej. cmd.system.quit"
											icon="close-circle"
											type="is-danger"
											@typing="getFilteredCommands"
										></b-taginput>
									</b-field>
								</div>
							</div>
						</div>
					</div>

					<!-- Access Rules Header -->
					<div class="rules-section-header mb-4 mt-5">
						<h3 class="title is-4 mb-2">Reglas de Acceso</h3>
						<p class="subtitle is-6 font-muted">Reglas especiales basadas en grupos de TeamSpeak, UIDs de usuario o direcciones IP.</p>
					</div>

					<!-- Rules Cards List -->
					<div v-if="model.rules.length === 0" class="empty-rules-box card mb-5">
						<div class="card-content has-text-centered p-6">
							<b-icon icon="alert-circle-outline" size="is-large" class="mb-3"></b-icon>
							<p class="title is-5 mb-2">No hay reglas de acceso</p>
							<p class="font-muted mb-4">Todos los usuarios operan bajo los permisos globales predeterminados.</p>
							<b-button type="is-brand" icon-left="plus" @click="createRule">Crear Primera Regla</b-button>
						</div>
					</div>

					<div v-else>
						<div v-for="(rule, idx) in model.rules" :key="idx" class="visual-rule-card card mb-5">
							<header class="card-header">
								<p class="card-header-title">
									<b-icon icon="shield-account" class="mr-2"></b-icon>
									Regla #{{ idx + 1 }}
								</p>
								<a class="card-header-icon" @click="deleteRule(idx)">
									<b-icon icon="delete" type="is-danger"></b-icon>
								</a>
							</header>
							<div class="card-content">
								<div class="columns is-multiline">
									<!-- Match criteria -->
									<div class="column is-12">
										<h5 class="subtitle is-6 mb-3"><strong>Criterios de Coincidencia (Filtros)</strong></h5>
										<div class="columns">
											<div class="column is-4">
												<b-field label="Grupos TS3 (IDs)" message="IDs numéricos de grupos de servidor.">
													<b-taginput
														v-model="rule.groups"
														placeholder="Ej. 6, 13"
														icon="account-group"
														allow-new
														type="is-info"
													></b-taginput>
												</b-field>
											</div>
											<div class="column is-4">
												<b-field label="Usuarios (UIDs)" message="UIDs de identidad de TeamSpeak.">
													<b-taginput
														v-model="rule.users"
														placeholder="Ej. 1hT5vEqxZ..."
														icon="account"
														allow-new
														type="is-success"
													></b-taginput>
												</b-field>
											</div>
											<div class="column is-4">
												<b-field label="Direcciones IP" message="IPs de llamadas API / localhost.">
													<b-taginput
														v-model="rule.ips"
														placeholder="Ej. 127.0.0.1, ::1"
														icon="ip-network"
														allow-new
														type="is-warning"
													></b-taginput>
												</b-field>
											</div>
										</div>
									</div>

									<!-- Permissions -->
									<div class="column is-12 border-top-dashed pt-4">
										<h5 class="subtitle is-6 mb-3"><strong>Privilegios Asignados</strong></h5>
										<div class="columns">
											<div class="column is-6">
												<b-field label="Permitir (+)" message="Comandos autorizados por esta regla.">
													<b-taginput
														v-model="rule.allowed"
														:data="filteredAutocompleteCommands"
														autocomplete
														open-on-focus
														allow-new
														placeholder="Ej. cmd.play, *"
														icon="check-circle"
														type="is-success"
														@typing="getFilteredCommands"
													></b-taginput>
												</b-field>
											</div>
											<div class="column is-6">
												<b-field label="Denegar (-)" message="Comandos bloqueados explícitamente por esta regla.">
													<b-taginput
														v-model="rule.denied"
														:data="filteredAutocompleteCommands"
														autocomplete
														open-on-focus
														allow-new
														placeholder="Ej. cmd.add"
														icon="close-circle"
														type="is-danger"
														@typing="getFilteredCommands"
													></b-taginput>
												</b-field>
											</div>
										</div>
									</div>
								</div>
							</div>
						</div>

						<!-- Add Rule Button -->
						<div class="has-text-centered mb-6 mt-5">
							<b-button type="is-brand" icon-left="plus" size="is-medium" @click="createRule">
								Agregar Nueva Regla de Acceso
							</b-button>
						</div>
					</div>
				</div>
			</b-tab-item>

			<!-- Tab 2: Code Editor -->
			<b-tab-item label="Editor TOML" icon="code-braces">
				<div class="editor-pane">
					<div class="pane-header">
						<h3><b-icon icon="file-document-edit" size="is-small"></b-icon> Edición Directa de rights.toml</h3>
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
			</b-tab-item>

			<!-- Tab 3: Commands Dictionary -->
			<b-tab-item label="Diccionario de Comandos" icon="book-search">
				<div class="commands-dictionary-layout card">
					<div class="card-content">
						<p class="pane-desc">Lista completa de comandos registrados en Dixibot. Podés buscarlos y ver sus nombres para agregarlos a las reglas.</p>
						<b-field>
							<b-input
								v-model="searchQuery"
								placeholder="Buscar comando (ej. play, volume, rights)"
								type="search"
								icon="magnify"
								clearable
							></b-input>
						</b-field>
						
						<div class="commands-scroll mt-4">
							<div
								v-for="cmd in filteredCommands"
								:key="cmd"
								class="command-item"
							>
								<span class="cmd-name" :title="cmd">{{ cmd }}</span>
								<b-button size="is-small" icon-left="content-copy" @click="copyCommand(cmd)">Copiar</b-button>
							</div>
							<div v-if="filteredCommands.length === 0" class="no-commands">
								No se encontraron comandos matching la búsqueda.
							</div>
						</div>
					</div>
				</div>
			</b-tab-item>
		</b-tabs>
	</div>
</template>

<script lang="ts">
import Vue from "vue";
import { Get } from "../Api";
import { ApiAuth } from "../ApiAuth";
import { Util } from "../Util";

interface ParsedRule {
	groups: string[];
	users: string[];
	ips: string[];
	allowed: string[];
	denied: string[];
}

interface SystemPermissions {
	globalAllowed: string[];
	globalDenied: string[];
	rules: ParsedRule[];
}

export default Vue.extend({
	data() {
		return {
			tomlContent: "",
			originalTomlContent: "",
			model: {
				globalAllowed: [],
				globalDenied: [],
				rules: []
			} as SystemPermissions,
			commandsList: [] as string[],
			autocompleteQuery: "",
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
		filteredAutocompleteCommands(): string[] {
			const query = this.autocompleteQuery.toLowerCase().trim();
			if (!query) return this.commandsList.slice(0, 50);
			return this.commandsList.filter(cmd => cmd.toLowerCase().includes(query)).slice(0, 50);
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
				if (response.status === 401) {
					window.localStorage.removeItem("api_auth");
					Get.AuthData = ApiAuth.Anonymous;
					window.location.hash = "/";
					return;
				}
				if (!response.ok) {
					throw new Error("No se pudo obtener el archivo de permisos.");
				}
				const data = await response.json();
				this.tomlContent = data.Value || "";
				this.originalTomlContent = this.tomlContent;
				
				// Parse to our visual model
				this.model = this.parseTomlToModel(this.tomlContent);
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
				if (response.status === 401) {
					window.localStorage.removeItem("api_auth");
					Get.AuthData = ApiAuth.Anonymous;
					window.location.hash = "/";
					return;
				}
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
			// If we are on visual editor, serialize model to tomlContent first
			if (this.activeTab === 0) {
				this.tomlContent = this.modelToToml(this.model);
			}

			if (!this.tomlContent.trim()) {
				this.$buefy.toast.open({
					message: "La configuración no puede estar vacía.",
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
				if (response.status === 401) {
					window.localStorage.removeItem("api_auth");
					Get.AuthData = ApiAuth.Anonymous;
					window.location.hash = "/";
					return;
				}

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
		onTabChange(newTab: number) {
			if (newTab === 0) {
				// Switched to Visual tab: parse from current tomlContent
				try {
					this.model = this.parseTomlToModel(this.tomlContent);
				} catch (err) {
					this.$buefy.toast.open({
						message: "El código TOML tiene errores. Arreglalos antes de cambiar a la vista visual.",
						type: "is-danger",
						duration: 4000
					});
					this.$nextTick(() => {
						this.activeTab = 1;
					});
				}
			} else if (newTab === 1) {
				// Switched to Code tab: serialize visual model
				this.tomlContent = this.modelToToml(this.model);
			}
		},
		createRule() {
			this.model.rules.push({
				groups: [],
				users: [],
				ips: [],
				allowed: [],
				denied: []
			});
			this.$buefy.toast.open({
				message: "Nueva regla de acceso creada.",
				type: "is-info"
			});
		},
		deleteRule(idx: number) {
			this.$buefy.dialog.confirm({
				title: "Eliminar Regla",
				message: `¿Estás seguro de que quieres eliminar la Regla #${idx + 1}?`,
				confirmText: "Eliminar",
				cancelText: "Cancelar",
				type: "is-danger",
				hasIcon: true,
				onConfirm: () => {
					this.model.rules.splice(idx, 1);
					this.$buefy.toast.open({
						message: "Regla eliminada de la lista.",
						type: "is-success"
					});
				}
			});
		},
		getFilteredCommands(text: string) {
			this.autocompleteQuery = text;
		},
		copyCommand(cmd: string) {
			navigator.clipboard.writeText(cmd).then(() => {
				this.$buefy.toast.open({
					message: `Copiado: ${cmd}`,
					type: "is-success",
					duration: 1500
				});
			});
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

		// TOML Parser & Serializer
		parseTomlToModel(toml: string): SystemPermissions {
			const model: SystemPermissions = {
				globalAllowed: [],
				globalDenied: [],
				rules: []
			};

			const lines = toml.split("\n");
			let currentRule: ParsedRule | null = null;
			let inGlobal = true;

			let currentKey: string | null = null;
			let accumulatedValues: string[] = [];
			let inArray = false;

			const commitValue = (key: string, values: string[]) => {
				if (inGlobal) {
					if (key === '"+"') {
						model.globalAllowed.push(...values);
					} else if (key === '"-"') {
						model.globalDenied.push(...values);
					}
				} else if (currentRule) {
					if (key === "groupid") {
						currentRule.groups.push(...values);
					} else if (key === "useruid") {
						currentRule.users.push(...values);
					} else if (key === "ip") {
						currentRule.ips.push(...values);
					} else if (key === '"+"') {
						currentRule.allowed.push(...values);
					} else if (key === '"-"') {
						currentRule.denied.push(...values);
					}
				}
			};

			for (let line of lines) {
				line = line.trim();
				if (line.startsWith("#") || line === "") continue;

				if (line.startsWith("[[rule]]")) {
					if (currentKey && accumulatedValues.length > 0) {
						commitValue(currentKey, accumulatedValues);
						currentKey = null;
						accumulatedValues = [];
						inArray = false;
					}

					inGlobal = false;
					if (currentRule) {
						model.rules.push(currentRule);
					}
					currentRule = {
						groups: [],
						users: [],
						ips: [],
						allowed: [],
						denied: []
					};
					continue;
				}

				if (inArray) {
					if (line.includes("]")) {
						const parts = line.split("]");
						const arrayPart = parts[0];
						accumulatedValues.push(...this.extractStrings(arrayPart));
						if (currentKey) {
							commitValue(currentKey, accumulatedValues);
						}
						currentKey = null;
						accumulatedValues = [];
						inArray = false;
						continue;
					} else {
						accumulatedValues.push(...this.extractStrings(line));
						continue;
					}
				}

				if (line.includes("=")) {
					const idx = line.indexOf("=");
					const key = line.substring(0, idx).trim();
					const val = line.substring(idx + 1).trim();

					if (val.startsWith("[")) {
						if (val.endsWith("]")) {
							const arrayContent = val.substring(1, val.length - 1);
							const values = this.extractStrings(arrayContent);
							commitValue(key, values);
						} else {
							currentKey = key;
							accumulatedValues = this.extractStrings(val.substring(1));
							inArray = true;
						}
					} else {
						const valStr = this.cleanString(val);
						commitValue(key, [valStr]);
					}
				}
			}

			if (currentKey && accumulatedValues.length > 0) {
				commitValue(currentKey, accumulatedValues);
			}
			if (currentRule) {
				model.rules.push(currentRule);
			}

			return model;
		},
		extractStrings(text: string): string[] {
			const result: string[] = [];
			const regex = /"([^"\\]*(?:\\.[^"\\]*)*)"/g;
			let match;
			let hasQuotes = false;
			while ((match = regex.exec(text)) !== null) {
				result.push(match[1]);
				hasQuotes = true;
			}
			if (!hasQuotes) {
				const parts = text.split(",");
				for (let part of parts) {
					part = part.trim();
					if (part) {
						if (part.startsWith('"') && part.endsWith('"')) {
							part = part.substring(1, part.length - 1);
						}
						result.push(part);
					}
				}
			}
			return result;
		},
		cleanString(val: string): string {
			let s = val.trim();
			if (s.startsWith('"') && s.endsWith('"')) {
				s = s.substring(1, s.length - 1);
			}
			return s;
		},
		modelToToml(model: SystemPermissions): string {
			let toml = "# Dixibot Configuration\n";
			toml += "# Rights declaration file\n";
			toml += "# Automatically managed via Dixibot Web Interface\n\n";

			// Global allowed
			if (model.globalAllowed.length > 0) {
				toml += '"+" = [\n';
				for (const cmd of model.globalAllowed) {
					toml += `\t"${cmd}",\n`;
				}
				toml += "]\n\n";
			}

			// Global denied
			if (model.globalDenied.length > 0) {
				toml += '"-" = [\n';
				for (const cmd of model.globalDenied) {
					toml += `\t"${cmd}",\n`;
				}
				toml += "]\n\n";
			}

			// Rules
			for (const rule of model.rules) {
				toml += "[[rule]]\n";
				
				// groupid
				const groupsStr = rule.groups.map(g => isNaN(Number(g)) ? `"${g}"` : g).join(", ");
				toml += `\tgroupid = [ ${groupsStr} ]\n`;

				// useruid
				const usersStr = rule.users.map(u => `"${u}"`).join(", ");
				toml += `\tuseruid = [ ${usersStr} ]\n`;

				// ip
				const ipsStr = rule.ips.map(ip => `"${ip}"`).join(", ");
				toml += `\tip = [ ${ipsStr} ]\n`;

				// allowed
				if (rule.allowed.length === 1 && rule.allowed[0] === "*") {
					toml += '\t"+" = "*"\n';
				} else if (rule.allowed.length > 0) {
					toml += '\t"+" = [\n';
					for (const cmd of rule.allowed) {
						toml += `\t\t"${cmd}",\n`;
					}
					toml += '\t]\n';
				}

				// denied
				if (rule.denied.length === 1 && rule.denied[0] === "*") {
					toml += '\t"-" = "*"\n';
				} else if (rule.denied.length > 0) {
					toml += '\t"-" = [\n';
					for (const cmd of rule.denied) {
						toml += `\t\t"${cmd}",\n`;
					}
					toml += '\t]\n';
				}

				toml += "\n";
			}

			return toml;
		}
	}
});
</script>

<style lang="less">
.permissions-page {
	padding-top: 1.5rem;
	padding-bottom: 4rem;
	max-width: 1240px;
	margin: 0 auto;
}

.permissions-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 2.5rem;
	border-bottom: 1px solid var(--line);
	padding-bottom: 1.75rem;

	.eyebrow {
		margin-bottom: 0.5rem;
	}

	.title {
		margin-top: 0.25rem !important;
		margin-bottom: 0.8rem !important;
		color: var(--text);
		font-weight: 900;
		letter-spacing: -0.03em;
	}

	.subtitle {
		color: var(--muted);
		margin-top: 0 !important;
	}

	.header-actions {
		display: flex;
		gap: 0.85rem;
		
		.button {
			box-shadow: 0 4px 12px rgba(0,0,0,0.1);
			
			&:hover {
				transform: translateY(-2px);
				box-shadow: 0 8px 20px rgba(0,0,0,0.15);
			}
		}
	}
}

.font-muted {
	color: var(--muted) !important;
}

.permissions-tabs {
	.tab-content {
		padding: 2rem 0;
	}
	
	.tabs {
		margin-bottom: 1.5rem;
		
		ul {
			border-bottom-color: var(--line);
		}
		
		li a {
			font-weight: 700;
			font-size: 1.05rem;
			padding: 0.75rem 1.5rem;
			transition: all 0.25s ease;
		}
	}
}

.visual-manager-layout {
	max-width: 960px;
	margin: 0 auto;
}

.global-perms-card {
	border-left: 5px solid var(--brand);
	position: relative;
	
	&::after {
		content: "Predeterminado";
		position: absolute;
		top: 1rem;
		right: 1.25rem;
		font-size: 0.7rem;
		font-weight: 800;
		text-transform: uppercase;
		letter-spacing: 0.08em;
		background: rgba(109, 93, 252, 0.15);
		color: var(--brand);
		padding: 0.25rem 0.6rem;
		border-radius: 99px;
	}
}

.visual-rule-card {
	background: var(--surface) !important;
	border: 1px solid var(--line);
	border-radius: 20px !important;
	overflow: hidden;
	box-shadow: var(--shadow);
	transition: all 0.3s ease;
	border-left: 5px solid var(--brand-2);
	margin-bottom: 2rem !important;
	
	&:hover {
		transform: translateY(-3px);
		box-shadow: 0 25px 60px rgba(0, 0, 0, 0.22);
	}
	
	.card-header {
		background: rgba(255, 255, 255, 0.01) !important;
		border-bottom: 1px solid var(--line);
		box-shadow: none !important;
		min-height: 52px;
		align-items: center;
		padding: 0.25rem 0.5rem;
		
		.card-header-title {
			color: var(--text);
			font-weight: 800;
			font-size: 1rem;
			letter-spacing: -0.01em;
		}

		.card-header-icon {
			padding: 0.5rem 1.25rem;
			border-radius: 99px;
			transition: background-color 0.2s ease;
			
			&:hover {
				background: rgba(241, 70, 104, 0.1);
			}
		}
	}
	
	.card-content {
		padding: 2rem;
		
		.label {
			color: var(--text) !important;
			font-size: 0.88rem;
			font-weight: 700;
			margin-bottom: 0.5rem;
		}
	}
}

.empty-rules-box {
	background: var(--surface) !important;
	border: 2px dashed var(--line);
	border-radius: 20px !important;
	color: var(--muted);
	padding: 3rem 1.5rem;
	transition: border-color 0.3s ease;
	
	&:hover {
		border-color: var(--brand);
	}
}

.border-top-dashed {
	border-top: 1px dashed var(--line);
}

.pt-4 {
	padding-top: 1.5rem !important;
}

/* Autocomplete/Taginput layout overrides */
.taginput .taginput-container {
	background: var(--surface-solid) !important;
	border: 1px solid var(--line) !important;
	border-radius: 12px !important;
	color: var(--text) !important;
	padding: 0.35rem 0.5rem !important;
	box-shadow: inset 0 2px 4px rgba(0,0,0,0.05) !important;
	transition: all 0.2s ease;
	
	&:focus-within {
		border-color: var(--brand) !important;
		box-shadow: 0 0 0 3px rgba(109, 93, 252, 0.14) !important;
	}
	
	.tag {
		border-radius: 8px !important;
		font-weight: 700;
		font-size: 0.78rem;
		padding: 0.35rem 0.65rem;
		height: auto;
		background: rgba(109, 93, 252, 0.12) !important;
		color: var(--brand) !important;
		border: 1px solid rgba(109, 93, 252, 0.2);
		
		&.is-success {
			background: rgba(16, 185, 129, 0.12) !important;
			color: #10b981 !important;
			border: 1px solid rgba(16, 185, 129, 0.2);
		}
		
		&.is-danger {
			background: rgba(244, 63, 94, 0.12) !important;
			color: #f43f5e !important;
			border: 1px solid rgba(244, 63, 94, 0.2);
		}
		
		&.is-warning {
			background: rgba(245, 158, 11, 0.12) !important;
			color: #f59e0b !important;
			border: 1px solid rgba(245, 158, 11, 0.2);
		}
		
		&.is-info {
			background: rgba(59, 130, 246, 0.12) !important;
			color: #3b82f6 !important;
			border: 1px solid rgba(59, 130, 246, 0.2);
		}

		.delete {
			margin-left: 0.35rem;
			background: rgba(0, 0, 0, 0.2);
			
			&:hover {
				background: rgba(0, 0, 0, 0.4);
			}
		}
	}
	
	input {
		color: var(--text) !important;
		background: transparent !important;
		font-size: 0.88rem;
	}
}

/* Autocomplete dropdown item list */
.autocomplete .dropdown-content {
	border-radius: 12px !important;
	border: 1px solid var(--line) !important;
	box-shadow: var(--shadow) !important;
	padding: 0.5rem !important;
	background: var(--surface-solid) !important;
	
	.dropdown-item {
		border-radius: 8px !important;
		font-family: monospace;
		font-weight: 600;
		font-size: 0.82rem;
		padding: 0.45rem 0.85rem !important;
		transition: all 0.2s ease;
		
		&.is-hovered, &:hover {
			background: rgba(109, 93, 252, 0.1) !important;
			color: var(--brand) !important;
		}
	}
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
		padding: 1.2rem 1.5rem;
		background: rgba(255, 255, 255, 0.02);
		border-bottom: 1px solid var(--line);
		display: flex;
		justify-content: space-between;
		align-items: center;

		h3 {
			font-weight: 800;
			color: var(--text);
			display: flex;
			align-items: center;
			gap: 0.5rem;
			font-size: 1rem;
		}

		.status-indicator {
			font-size: 0.75rem;
			font-weight: 800;
			text-transform: uppercase;
			letter-spacing: 0.08em;
			color: var(--muted);
			background: rgba(0, 0, 0, 0.06);
			padding: 0.3rem 0.65rem;
			border-radius: 6px;

			&.is-dirty {
				color: var(--brand);
				background: rgba(109, 93, 252, 0.12);
			}
		}
	}

	.toml-textarea {
		width: 100%;
		height: 60vh;
		min-height: 520px;
		background: transparent;
		border: none;
		color: var(--text);
		font-family: "Fira Code", Consolas, Monaco, "Andale Mono", "Ubuntu Mono", monospace;
		font-size: 0.95rem;
		line-height: 1.65;
		padding: 1.5rem;
		resize: vertical;
		outline: none;

		&:focus {
			box-shadow: none;
		}
	}
}

.commands-dictionary-layout {
	background: var(--surface) !important;
	border: 1px solid var(--line);
	border-radius: 20px;
	overflow: hidden;
	box-shadow: var(--shadow);
	max-width: 900px;
	margin: 0 auto;

	.pane-desc {
		font-size: 0.9rem;
		color: var(--muted);
		margin-bottom: 1.5rem;
		line-height: 1.5;
	}

	.commands-scroll {
		max-height: 55vh;
		overflow-y: auto;
		display: grid;
		grid-template-columns: repeat(2, 1fr);
		gap: 0.75rem;
		padding-right: 0.5rem;
		
		@media(max-width: 768px) {
			grid-template-columns: 1fr;
		}
	}

	.command-item {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 0.75rem 1rem;
		background: var(--surface-solid);
		border: 1px solid var(--line);
		border-radius: 12px;
		box-shadow: 0 2px 4px rgba(0,0,0,0.02);
		transition: all 0.25s ease;
		
		&:hover {
			border-color: var(--brand);
			transform: translateY(-2px);
			box-shadow: 0 6px 15px rgba(109,93,252,0.1);
			
			.cmd-name {
				color: var(--brand);
			}
		}

		.cmd-name {
			font-family: monospace;
			font-weight: 700;
			color: var(--text);
			font-size: 0.85rem;
			overflow: hidden;
			text-overflow: ellipsis;
			white-space: nowrap;
		}
		
		.button {
			height: auto;
			padding: 0.35rem 0.75rem;
			font-size: 0.75rem;
			border-radius: 8px !important;
		}
	}

	.no-commands {
		text-align: center;
		color: var(--muted);
		padding: 3rem 0;
		grid-column: 1 / -1;
	}
}

.button.is-brand {
	background: linear-gradient(135deg, var(--brand), #8b5cf6) !important;
	border: 0 !important;
	color: #fff !important;
	font-weight: 700;
	box-shadow: 0 4px 14px rgba(109, 93, 252, 0.25);
	
	&:hover {
		transform: translateY(-2px);
		box-shadow: 0 8px 22px rgba(109, 93, 252, 0.35);
	}
}
</style>
