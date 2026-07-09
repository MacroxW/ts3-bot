<template>
	<b-collapse v-show="is_visible" class="card settings-group" aria-id="settings_box">
		<div
			slot="trigger"
			slot-scope="props"
			class="card-header"
			role="button"
			aria-controls="settings_box"
		>
			<p class="card-header-title">{{label}}</p>
			<a class="card-header-icon">
				<b-icon :icon="props.open ? 'menu-down' : 'menu-up'"></b-icon>
			</a>
		</div>
		<div class="card-content">
			<div class="content">
				<slot></slot>
			</div>
		</div>
	</b-collapse>
</template>

<script lang="ts">
import Vue from "vue";

export default Vue.component("settings-group", {
	props: {
		label: {
			type: String,
			required: false
		}
	},
	data() {
		return {
			children: [] as boolean[]
		};
	},
	computed: {
		is_visible(): boolean {
			return this.children.length > 0 && this.children.find(x => x) === true;
		}
	}
});
</script>

<style lang="less">
.settings-group{overflow:hidden;border-radius:16px!important}.settings-group .card-header{min-height:48px;align-items:center;background:transparent!important;box-shadow:none!important;border-bottom:1px solid var(--line)}.settings-group .card-header-title{padding:.8rem 1rem;font-size:.92rem}.settings-group .card-header-icon{width:46px;color:var(--muted)}.settings-group .card-content{padding:.35rem 1rem}.settings-group .content{margin:0}.settings-group .content>.settings-field:last-child{border-bottom:0}
</style>
