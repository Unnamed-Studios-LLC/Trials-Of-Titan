entity_state(name: "Overworld Balun", actions: {
	stay_near_spawn(speed: 12, distance: 12, enforce: 4, period: 10)
	state(name: "wander", actions: {
		add_effect(type: "Invulnerable", duration: 10, period: 9)
		wander(speed: 3, period: 1)
		if_player_within(distance: 10, true: {
			if_elapsed(sec: 5, true: {
				set_state(name: "init-fight", parent: 1)
			})
		})
	})
	state(name: "init-fight", actions: {
		remove_effect(type: "Invulnerable")
		set_state(name: "random-fight", parent: 1)
	})
	state(name: "random-fight", actions: {
		wander(speed: 2, period: 0.5)
		set_texture(index: 0)

		shoot(index: 1, angle: 0, angleGap: 60, amount: 6, period: 1.2)
		shoot(index: 1, angle: 40, angleGap: 60, amount: 6, period: 1.2, delay: 0.4)
		shoot(index: 1, angle: 20, angleGap: 60, amount: 6, period: 1.2, delay: 0.8)

		if_elapsed(sec: 5, true: {
			random_state(parent: 1, name: "fight-2", name: "fight-2")
		})
	})
	state(name: "fight-1", actions: {
		wander(speed: 2, period: 0.5)
		set_texture(index: 1)
		
		throw_minion_player(name: "Lightning", offset: 0.2, rand: 1.5, duration: 0.1, period: 0.2, target: "Random")

		if_elapsed(sec: 8, true: {
			set_state(name: "random-fight", parent: 1)
		})
	})
	state(name: "fight-2", actions: {
		set_state(name: "fight-2-charge", parent: 1)
	})
	state(name: "fight-2-charge", actions: {
		set_texture(index: 2)
		set_flash(duration: 1.5)
		charge(offset: 0, delay: 1.4, speed: 0.2)
		set_size(value: 1.6)
		if_elapsed(sec: 1.6, true: {
			set_texture(index: 3)
			set_state(name: "fight-2-slam", parent: 1)
		})
	})
	state(name: "fight-2-slam", actions: {
		set_size(value: 1.3)
		shoot(index: 3, angle: 0, angleGap: 30, amount: 12)
		shoot(index: 3, angle: 20, angleGap: 30, amount: 12, delay: 0.4)
		shoot(index: 3, angle: 10, angleGap: 30, amount: 12, delay: 0.8)

		if_elapsed(sec: 1.2, true: {
			set_state(name: "random-fight", parent: 1)
		})
	})
}, death: {
	create_gate(type: "dumir")
})
entity_state(name: "Balun", actions: {
	set_music(name: "Balun's_Might")
	state(name: "inactive", actions: {
		wander(speed: 1.4, period: 0.4)
		stay_near_spawn(speed: 6, distance: 4, enforce: 4, period: 8)
		if_health_below(percent: 0.98, true: {
			set_state(name: "switch", parent: 1)
		})
	})
	state(name: "switch", actions: {
		set_texture(index: 0)
		wander(speed: 1.4, period: 0.4)
		stay_near_spawn(speed: 6, distance: 4, enforce: 4, period: 8)
		if_elapsed(sec: 2, true: {
			random_state(name: "charge_and_slam", name: "lightning", parent: 1)
		})
	})
	state(name: "lightning", actions: {
		set_texture(index: 1)
		set_flash(color: "white", duration: 10)
		spawn(name: "Balun's Lightning", rate: 2, max: 8, period: 2.4, delay: 1, selection: "RandomPlayer", searchRadius: 12)
		shoot(index: 0, amount: 6, angle: 0, period: 2, delay: 0)
		shoot(index: 0, amount: 6, angle: 30, period: 2, delay: 1)

		if_elapsed(sec: 14, true: {
			set_state(name: "switch", parent: 1)
		})
	})
	state(name: "charge_and_slam", actions: {
		state(name: "charge", actions: {
			set_texture(index: 2)
			set_flash(duration: 1.9)
			set_size(value: 1.6)
			charge(offset: 0, delay: 1.65, speed: 0.3)

			if_elapsed(sec: 2, true: {
				set_texture(index: 3)
				set_state(name: "slam", parent: 1)
			})
		})
		state(name: "slam", actions: {
			set_size(value: 1.3)
			shoot(index: 3, amount: 12, delay: 0.5)
			shoot(index: 2, amount: 6, delay: 0.9)
			if_elapsed(sec: 1.5, true: {
				set_state(name: "charge", parent: 1)
			})
		})
		if_elapsed(sec: 13.9, true: {
			set_size(value: 1.3)
			set_state(name: "switch", parent: 1)
		})
	})
}, death: {
	create_return_portal()
})
entity_state(name: "Balun's Lightning", actions: {
	if_elapsed(sec: 4, true: {
		set_texture(index: 2)
		if_elapsed(sec: 0.4, true: {
			set_ground_object()
		})
		shoot(index: 1, amount: 4, angle: 45, delay: 0.47)

		shoot(amount: 4, offsetX: 2, offsetY: 0, delay: 1.1)
		shoot(amount: 4, offsetX: 0, offsetY: -2, delay: 1.1)
		shoot(amount: 4, offsetX: -2, offsetY: 0, delay: 1.1)
		shoot(amount: 4, offsetX: 0, offsetY: 2, delay: 1.1)
		if_elapsed(sec: 1.95, true: {
			despawn()
		})
	}, false: {
		chase(speed: 3)
	})
})