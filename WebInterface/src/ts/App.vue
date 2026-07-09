<template>
	<div id="app">
		<Navbar />
		<section class="section">
			<div class="container">
				<router-view />
			</div>
		</section>
	</div>
</template>

<script lang="ts">
import Navbar from "./Components/Navbar.vue";

export default {
	data() {
		return {};
	},
	created() {
		const theme = localStorage.getItem("theme");
		if (theme === "dark") {
			document.documentElement.classList.add("theme-dark");
		}
	},
	components: {
		Navbar,
	}
};
</script>

<style lang="less">
html {
	//overflow-y: auto !important;
	transition: background-color 0.3s ease, color 0.3s ease;
}

/* Dark Mode Overrides */
html.theme-dark {
	background-color: #121212 !important;
	color: #e0e0e0 !important;

	body {
		background-color: #121212 !important;
		color: #e0e0e0 !important;
	}

	.navbar {
		background-color: #1a1a1a !important;
		box-shadow: 0 2px 0 0 #2c2c2c !important;
	}

	.navbar-menu {
		background-color: #1a1a1a !important;
	}

	.navbar-item {
		color: #e0e0e0 !important;
	}

	.navbar-item:hover,
	.navbar-item.is-active,
	.navbar-link:hover,
	.navbar-link.is-active {
		background-color: #2c2c2c !important;
		color: #ffffff !important;
	}

	.hero.is-primary {
		background-color: #2d1b4e !important;
		color: #ffffff !important;
	}

	.title {
		color: #ffffff !important;
	}

	.subtitle {
		color: #b5b5b5 !important;
	}

	.box,
	.card {
		background-color: #1a1a1a !important;
		color: #e0e0e0 !important;
		box-shadow: 0 0.5em 1em -0.125em rgba(0,0,0, 0.2), 0 0px 0px 1px rgba(255,255,255, 0.05) !important;
	}

	.card-header {
		background-color: #242424 !important;
		border-bottom: 1px solid #333 !important;
	}

	.card-header-title {
		color: #ffffff !important;
	}

	.card-footer {
		border-top: 1px solid #333 !important;
		background-color: #1e1e1e !important;
	}

	.card-footer-item {
		border-right: 1px solid #333 !important;
		color: #8c8c8c !important;
	}

	.card-footer-item:hover {
		background-color: #2c2c2c !important;
		color: #ffffff !important;
	}

	.label {
		color: #ffffff !important;
	}

	.input,
	.textarea,
	.select select {
		background-color: #242424 !important;
		border-color: #3e3e3e !important;
		color: #e0e0e0 !important;
	}

	.input:focus,
	.textarea:focus,
	.select select:focus {
		border-color: #7957d5 !important;
	}

	.input::placeholder {
		color: #707070 !important;
	}

	.button {
		background-color: #242424 !important;
		border-color: #3e3e3e !important;
		color: #e0e0e0 !important;
	}

	.button:hover {
		background-color: #2c2c2c !important;
		color: #ffffff !important;
		border-color: #4e4e4e !important;
	}

	.button.is-primary {
		background-color: #7957d5 !important;
		border-color: transparent !important;
		color: #ffffff !important;
	}

	.button.is-primary:hover {
		background-color: #6943cc !important;
	}

	.button.is-success {
		background-color: #48c774 !important;
		border-color: transparent !important;
		color: #ffffff !important;
	}

	.button.is-success:hover {
		background-color: #3ec46d !important;
	}

	.button.is-danger {
		background-color: #f14668 !important;
		border-color: transparent !important;
		color: #ffffff !important;
	}

	.button.is-danger:hover {
		background-color: #f03a5f !important;
	}

	.button.is-static {
		background-color: #1a1a1a !important;
		border-color: #3e3e3e !important;
		color: #8c8c8c !important;
	}

	.checkbox:hover,
	.radio:hover {
		color: #ffffff !important;
	}

	.table {
		background-color: #1a1a1a !important;
		color: #e0e0e0 !important;
	}

	.table th {
		color: #ffffff !important;
		border-color: #2c2c2c !important;
	}

	.table td {
		border-color: #2c2c2c !important;
	}

	.table.is-hoverable tbody tr:hover {
		background-color: #242424 !important;
	}

	.modal-card-head,
	.modal-card-foot {
		background-color: #242424 !important;
		border-color: #333333 !important;
	}

	.modal-card-body {
		background-color: #1a1a1a !important;
		color: #e0e0e0 !important;
	}

	.modal-card-title {
		color: #ffffff !important;
	}

	.dropdown-content {
		background-color: #242424 !important;
		box-shadow: 0 8px 8px rgba(0,0,0, 0.4), 0 0 0 1px rgba(255,255,255, 0.05) !important;
	}

	.dropdown-item {
		color: #e0e0e0 !important;
	}

	a.dropdown-item:hover {
		background-color: #2c2c2c !important;
		color: #ffffff !important;
	}

	.panel-heading {
		background-color: #242424 !important;
		color: #ffffff !important;
		border-color: #333333 !important;
	}

	.panel-block {
		border-color: #333333 !important;
		color: #e0e0e0 !important;
	}

	.panel-block:hover {
		background-color: #242424 !important;
	}

	.panel-block.is-active {
		color: #7957d5 !important;
	}

	.pagination-link,
	.pagination-next,
	.pagination-previous {
		background-color: #242424 !important;
		border-color: #3e3e3e !important;
		color: #e0e0e0 !important;
	}

	.pagination-link:hover,
	.pagination-next:hover,
	.pagination-previous:hover {
		background-color: #2c2c2c !important;
		color: #ffffff !important;
	}

	.pagination-link.is-current {
		background-color: #7957d5 !important;
		border-color: transparent !important;
		color: #ffffff !important;
	}

	.menu-label {
		color: #8c8c8c !important;
	}

	.menu-list a {
		color: #e0e0e0 !important;
	}

	.menu-list a:hover {
		background-color: #2c2c2c !important;
		color: #ffffff !important;
	}

	.menu-list a.is-active {
		background-color: #7957d5 !important;
		color: #ffffff !important;
	}
}
</style>
