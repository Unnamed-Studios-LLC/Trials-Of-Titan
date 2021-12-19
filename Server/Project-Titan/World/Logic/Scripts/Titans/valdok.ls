entity_state(name: "Overworld Valdok", actions: {
	state(name: "inactive", actions: {
		add_effect(type: "Fortified", duration: 1, period: 0.8)
		stay_near_spawn(speed: 6, distance: 20, enforce: 4, period: 8)
		wander(speed: 1.4, period: 0.3)
		if_health_below(percent: 0.97, true: {
			set_state(name: "switch", parent: 1)
		})
	})
	state(name: "switch", actions: {
		add_effect(type: "Fortified", duration: 1, period: 0.8)
		wander(speed: 2, period: 0.4)
		if_elapsed(sec: 2, true: {
			random_state(name: "rocks", name: "chase", parent: 1)
		})
	})
	state(name: "rocks", actions: {
		wander(speed: 1.4, period: 0.3)
		shoot(index: 6, amount: 4, angle: 105, period: 1.2, delay: 1.5)
		shoot(index: 6, amount: 4, angle: 75, period: 1.2, delay: 1.5)
		shoot_player(index: 0, amount: 13, period: 0.9)
		if_elapsed(sec: 7, true: {
			set_state(name: "switch", parent: 1)
		})
	})
	state(name: "chase", actions: {
		wander(speed: 1, period: 0.5)
		chase(speed: 3, min: 1)
		shoot_player(index: 6, amount: 6, angleGap: 20, period: 1.4, delay: 1)
		shoot_player(index: 5, amount: 7, angleGap: 18, period: 1.4, delay: 0.8)
		shoot_player(index: 5, amount: 12, angleGap: 16, angleOffset: 180, period: 1.4, delay: 1)
		if_elapsed(sec: 9, true: {
			set_state(name: "switch", parent: 1)
		})
	})
}, death: {
	create_gate(type: "forge")
})
entity_state(name: "Valdok", actions: {
	if_health_below(percent: 0.35, trigger: "yes", true: {
		set_state(name: "transform")
	})
	state(name: "inactive", actions: {
		set_music(name: "Valdoks_Wrath")
		add_effect(type: "Fortified", duration: 1, period: 0.8)
		wander(speed: 1, period: 0.4)
		stay_near_spawn(speed: 4, distance: 2, enforce: 4, period: 6)
		if_health_below(percent: 0.98, true: {
			set_state(name: "easy_switch", parent: 1)
		})
	})
	state(name: "easy_switch", actions: {
		add_effect(type: "Fortified", duration: 1, period: 0.8)
		wander(speed: 2, period: 0.4)
		stay_near_spawn(speed: 8, distance: 3, enforce: 4, period: 6)
		if_elapsed(sec: 2, true: {
			random_state(name: "easy_rings", name: "easy_boomerang", parent: 1)
		})
	})
	state(name: "easy_rings", actions: {
		stay_near_spawn(speed: 10, distance: 0, enforce: 1)
		shoot(index: 4, amount: 3, period: 0.44)
		shoot(index: 0, angle: 0, amount: 12, period: 2, delay: 2)
		shoot(index: 0, angle: 15, amount: 12, period: 2, delay: 3)
		if_elapsed(sec: 16, true: {
			set_state(name: "easy_switch", parent: 1)
		})
		spawn(name: "Valdok's Rock Bomb", rate: 1, max: 3, period: 1)
	})
	state(name: "easy_boomerang", actions: {
		stay_near_spawn(speed: 10, distance: 0, enforce: 1)
		shoot_spiral(index: 5, amount: 2, angle: 0, angleStep: 7.714, period: 0.15, delay: 2)
		shoot(index: 6, amount: 4, angle: 105, period: 1.75, delay: 2)
		shoot(index: 6, amount: 4, angle: 75, period: 1.75, delay: 2)
		if_elapsed(sec: 16, true: {
			set_state(name: "easy_switch", parent: 1)
		})
		spawn(name: "Valdok's Rock Bomb", rate: 1, max: 1, period: 2)
	})
	state(name: "transform", actions: {
		set_music(name: "Valdoks_Wrath_Trans")
		add_effect(type: "Invulnerable", duration: 1, period: 0.8)
		set_texture(index: 1)
		if_elapsed(sec: 2.5, true: {
			set_texture(index: 2)
			set_state(name: "trans_switch", parent: 1)
		})
	})
	state(name: "trans_switch", actions: {
		add_effect(type: "Fortified", duration: 1, period: 0.8)
		wander(speed: 3, period: 0.4)
		stay_near_spawn(speed: 8, distance: 3, enforce: 4, period: 6)
		if_elapsed(sec: 2, true: {
			random_state(name: "trans_blasts", parent: 1)
		})
	})
	state(name: "trans_blasts", actions: {
		wander(speed: 3, period: 0.4)
		stay_near_spawn(speed: 3, distance: 3, enforce: 3, period: 4)

		shoot(index: 5, amount: 10, angle: 90, angleGap: 9, period: 4, delay: 1)
		shoot(index: 5, amount: 10, angle: 270, angleGap: 9, period: 4, delay: 1)

		shoot(index: 5, amount: 10, angle: 0, angleGap: 9, period: 4, delay: 3)
		shoot(index: 5, amount: 10, angle: 180, angleGap: 9, period: 4, delay: 3)

		if_elapsed(sec: 16, true: {
			set_state(name: "trans_switch", parent: 1)
		})
		spawn(name: "Valdok's Rock Bomb", rate: 1, max: 2, period: 2)
	})
	state(name: "trans_cracks", actions: {
		wander(speed: 3, period: 0.4)
		stay_near_spawn(speed: 8, distance: 3, enforce: 4, period: 6)
		crack_ground(tile: "Valdok's Lava", joints: 2, jointAngleMin: -20, jointAngleMax: 20, rate: 10, radius: 10)
		crack_ground(tile: "Valdok's Lava", joints: 2, jointAngleMin: -20, jointAngleMax: 20, rate: 10, radius: 10)
		crack_ground(tile: "Valdok's Lava", joints: 2, jointAngleMin: -20, jointAngleMax: 20, rate: 10, radius: 10)
		crack_ground(tile: "Valdok's Lava", joints: 2, jointAngleMin: -20, jointAngleMax: 20, rate: 10, radius: 10)
		shoot_player(index: 6, amount: 8, period: 1.6)
		shoot_player(index: 6, amount: 7, periodMin: 1.3, periodMax: 1.8)
		shoot(index: 1, angleMin: 0, angleMax: 360, amount: 16, periodMin: 1, periodMax: 1.6)
		if_elapsed(sec: 8, true: {
			set_state(name: "trans_switch", parent: 1)
		})
	})
}, death: {
	create_return_portal()
})
entity_state(name: "Valdok's Rock Bomb", actions: {
	if_elapsed(sec: 2, true: {
		set_flash(duration: 10)
		if_elapsed(sec: 2, true: {
			shoot(amount: 12)
			crack_ground(tile: "Valdok's Lava", joints: 2, jointAngleMin: -20, jointAngleMax: 20, rate: 10, radius: 5)
			crack_ground(tile: "Valdok's Lava", joints: 2, jointAngleMin: -20, jointAngleMax: 20, rate: 10, radius: 5)
			crack_ground(tile: "Valdok's Lava", joints: 2, jointAngleMin: -20, jointAngleMax: 20, rate: 10, radius: 5)
			crack_ground(tile: "Valdok's Lava", joints: 2, jointAngleMin: -20, jointAngleMax: 20, rate: 10, radius: 5)
			if_elapsed(sec: 0.01, true: {
				despawn()
			}, false: {
				chase(speed: 3, min: 0, searchRadius: 14)
			})
		}, false: {
			chase(speed: 3, min: 0, searchRadius: 14)
		})
	}, false: {
		chase(speed: 3, min: 0, searchRadius: 14)
	})
})