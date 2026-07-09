<template>
	<div v-show="is_visible" class="field settings-field">
		<label class="label">{{label}}</label>
		<!-- <div class="field-label is-normal">
			<label class="label">{{label}}</label>
		</div> -->
		<div class="field-body">
			<b-field :grouped="grouped">
				<slot />
			</b-field>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from "vue";
import SettingsGroup from "./SettingsGroup.vue";
import { SettLevel, ISettFilter } from "../Model/SettingsLevel";

export default Vue.component("settings-field", {
	props: {
		filter: {
			type: Object as () => ISettFilter,
			required: false
		},
		path: { type: String, required: true },
		label: { type: String, required: true },
		expert: { type: Boolean, required: false, default: false },
		advanced: { type: Boolean, required: false, default: false },
		grouped: { type: Boolean, required: false, default: false }
	},
	created() {
		this.parentIndex = this.parent_arr.length;
		this.parent_arr.push(this.is_visible);
	},
	computed: {
		is_visible(): boolean {
			if (this.advanced && this.filter.level < SettLevel.Advanced) return false;
			if (this.expert && this.filter.level < SettLevel.Expert) return false;
			const low_filter = this.filter.text.toLowerCase();
			return (
				this.path.toLowerCase().indexOf(low_filter) >= 0 ||
				this.label.toLowerCase().indexOf(low_filter) >= 0
			);
		},
		parent_arr(): boolean[] {
			return (this.$parent.$parent.$data as any).children as boolean[];
		}
	},
	watch: {
		is_visible(val) {
			Vue.set(this.parent_arr, this.parentIndex, val);
		}
	},
	data() {
		return {
			SettLevel,

			parentIndex: 0
		};
	},
	methods: {}
});
</script>

<style lang="less">
.settings-field{display:grid;grid-template-columns:minmax(150px,32%) minmax(0,1fr);align-items:center;gap:1rem;margin:0!important;padding:.7rem 0;border-bottom:1px solid var(--line)}.settings-field>.label{margin:0!important;color:var(--muted)!important;font-size:.78rem;line-height:1.3}.settings-field>.field-body{display:block;min-width:0}.settings-field>.field-body>.field{margin:0}.settings-field .field.is-grouped{display:flex;align-items:center;flex-wrap:wrap}.settings-field .field.is-grouped>.control.is-expanded{min-width:180px}.settings-field .b-slider{width:100%}@media(max-width:650px){.settings-field{grid-template-columns:1fr;gap:.45rem;padding:.75rem 0}.settings-field>.label{color:var(--text)!important}.settings-field .field.is-grouped>.control{width:100%}.settings-field .field.is-grouped>.control .button{width:100%}}
</style>
