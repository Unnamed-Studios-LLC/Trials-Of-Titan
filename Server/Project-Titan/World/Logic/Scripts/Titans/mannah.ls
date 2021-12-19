entity_state(name: "Overworld Mannah", actions: {
	spawn(name: "Royal Scout", rate: 8, max: 8, radius: 8)
	spawn(name: "Royal Scout", rate: 2, max: 8, period: 4, radius: 8)
	state(name: "fight", actions: {
		wander(speed: 2, period: 0.3)
		stay_near_spawn(speed: 6, distance: 20, enforce: 4, period: 8)
		shoot_player(index: 2, amount: 16, period: 4, delay: 1)
		shoot_player(index: 8, amount: 3, angleGap: 16, periodMin: 1.2, periodMax: 2.0, delay: 0.6)
		if_elapsed(sec: 12, true: {
			set_state(name: "chase", parent: 1)
		})
	})
	state(name: "chase", actions: {
		state(name: "fast", actions: {
			chase(speed: 10, min: 2)
			if_elapsed(sec: 1, true: {
				set_state(name: "stop", parent: 1)
			})
		})
		state(name: "stop", actions: {
			chase(speed: 2, min: 1)
			if_elapsed(sec: 1, true: {
				set_state(name: "fast", parent: 1)
			})
		})
		if_elapsed(sec: 12, true: {
			set_state(name: "fight", parent: 1)
		})
		shoot_player(index: 8, amount: 8, angleGap: 20, period: 1.8, delay: 0.2)
		shoot_player(index: 1, amount: 4, angleGap: 32, period: 1.8, delay: 0.6)
		shoot_player(index: 9, amount: 9, period: 1.8, delay: 1)
	})
}, death: {
	create_gate(type: "fortress")
})
entity_state(name: "Mannah", actions: {
	if_elapsed(sec: 1, true: {
		set_music(name: "Mannah_The_Malevolent")
	})
	if_health_below(percent: 0.6, trigger: "yes", true: {
		set_state(name: "pre_blades")
	})
	if_health_below(percent: 0.15, trigger: "yes", true: {
		set_state(name: "pre_rage")
	})
	state(name: "passive", actions: {
		if_health_below(percent: 0.99, true: {
			set_state(name: "activate", parent: 1)
		})
		if_player_within(distance: 8, true: {
			set_state(name: "activate", parent: 1)
		})
	})
	state(name: "activate", actions: {
		add_effect(type: "Invulnerable")
		wander(speed: 2, period: 0.3)
		stay_near_spawn(speed: 4, distance: 3, enforce: 1, period: 2)
		taunt(text: "Your eagerness to defeat me is amusing.")
		if_elapsed(sec: 3, true: {
			set_state(name: "state_switch", parent: 1)
		})
	})
	state(name: "state_switch", actions: {
		set_texture(index: 0)
		taunt(text: "Your journey ends here.", text: "Your souls will enable my everlasting dominion!", text: "I will feed on your souls.")
		wander(speed: 2, period: 0.3)
		stay_near_spawn(speed: 4, distance: 3, enforce: 1, period: 2)
		set_flash(duration: 4)
		shoot_player(index: 10, amount: 5, angleOffsetMin: 0, angleOffsetMax: 360, periodMin: 0.8, periodMax: 1.2)
		if_elapsed(sec: 4, true: {
			random_state(name: "pre_spikes", name: "pre_spirals", parent: 1)
		})
	})
	state(name: "pre_spikes", actions: {
		move_to_region(speed: 4, region: "Tag7")
		shoot(index: 1, amount: 20)
		shoot_player(index: 3, amount: 8, angleOffsetMin: 0, angleOffsetMax: 360, period: 1.2, delay: 0.4)
		set_texture(index: 2)
		if_elapsed(sec: 2, true: {
			set_state(name: "spikes", parent: 1)
		})
	})
	state(name: "spikes", actions: {
		wander(speed: 2, period: 0.3)
		set_texture(index: 0)
		stay_near_spawn(speed: 4, distance: 3, enforce: 1, period: 2)
		shoot_player(index: 3, amount: 4, angleOffsetMin: 0, angleOffsetMax: 360, period: 1.2, delay: 0.2)

		state(name: "no_rotation", actions: {
			shoot_player(index: 2, amount: 7, angleOffsetMin: 0, angleOffsetMax: 360, period: 0.8, delay: 0.2)
			shoot(index: 1, amount: 4, angle: 45, period: 0.36)
			if_elapsed(sec: 3, true: {
				set_state(name: "rotate_to_45", parent: 1)
			})
		})
		state(name: "rotate_to_45", actions: {
			shoot_spiral(index: 1, amount: 4, angle: 50, angleStep: 5, period: 0.15)
			if_elapsed(sec: 1.05, true: {
				set_state(name: "rotation", parent: 1)
			})
		})
		state(name: "rotation", actions: {
			shoot_player(index: 2, amount: 7, angleOffsetMin: 0, angleOffsetMax: 360, period: 0.8, delay: 0.2)
			shoot(index: 1, amount: 4, angle: 0, period: 0.36)
			if_elapsed(sec: 3, true: {
				set_state(name: "rotate_to_0", parent: 1)
			})
		})
		state(name: "rotate_to_0", actions: {
			shoot_spiral(index: 1, amount: 4, angle: 5, angleStep: 5, period: 0.15)
			if_elapsed(sec: 1.05, true: {
				set_state(name: "no_rotation", parent: 1)
			})
		})
		if_elapsed(sec: 10, true: {
			set_state(name: "state_switch", parent: 1)
		})
	})
	state(name: "pre_spirals", actions: {
		move_to_region(speed: 4, region: "Tag7")
		shoot(index: 10, amount: 20)
		shoot_player(index: 10, amount: 18, delay: 0.2)
		shoot_player(index: 10, amount: 20, delay: 0.4)
		set_texture(index: 2)
		if_elapsed(sec: 2, true: {
			set_state(name: "spirals", parent: 1)
		})
	})
	state(name: "spirals", actions: {
		wander(speed: 2, period: 0.3)
		set_texture(index: 0)
		stay_near_spawn(speed: 4, distance: 3, enforce: 1, period: 2)
		shoot_player(index: 2, amount: 18, period: 3.2)
		shoot(index: 0, amount: 4, angle: 0, angleGap: 0, period: 3.2, delay: 1)
		shoot(index: 0, amount: 4, angle: 45, angleGap: 0, period: 3.2, delay: 2)
		if_elapsed(sec: 10, true: {
			set_state(name: "state_switch", parent: 1)
		})
	})
	state(name: "pre_blades", actions: {
		add_effect(type: "Invincible", duration: 2, period: 1)
		taunt(text: "I must prepare..")
		move_to_region(speed: 4, region: "Tag7")
		shoot(index: 8, amount: 20)
		set_texture(index: 2)
		if_elapsed(sec: 2, true: {
			set_state(name: "blades", parent: 1)
		})
	})
	state(name: "blades", actions: {
		set_flash(duration: 0)
		add_effect(type: "Invincible", duration: 2, period: 1)
		set_texture(index: 3)
		spawn(name: "Soul Crux", rate: 1, max: 4, radius: 0.3, arc: 45)
		spawn(name: "Soul Crux", rate: 1, max: 4, radius: 0.3, arc: 135)
		spawn(name: "Soul Crux", rate: 1, max: 4, radius: 0.3, arc: 225)
		spawn(name: "Soul Crux", rate: 1, max: 4, radius: 0.3, arc: 315)
		set_state(name: "blades_rotation", parent: 1)
	})
	state(name: "blades_rotation", actions: {
		add_effect(type: "Invincible", duration: 2, period: 1)
		shoot_player(index: 9, amount: 5, angleOffsetMin: 0, angleOffsetMax: 360, periodMin: 0.9, periodMax: 1.3)
		if_elapsed(secMin: 3, secMax: 6, true: {
			set_state(name: "blades_no_rotation", parent: 1)
		})
		if_minion_count_under(count: 1, true: {
			set_state(name: "ascend", parent: 1)
		})
	})
	state(name: "blades_no_rotation", actions: {
		add_effect(type: "Invincible", duration: 2, period: 1)
		shoot_player(index: 9, amount: 5, angleOffsetMin: 0, angleOffsetMax: 360, periodMin: 0.9, periodMax: 1.3)
		if_elapsed(secMin: 3, secMax: 6, true: {
			set_state(name: "blades_rotation", parent: 1)
		})
		if_minion_count_under(count: 1, true: {
			set_state(name: "ascend", parent: 1)
		})
	})
	state(name: "ascend", actions: {
		set_flash(duration: 5)
		move_to_region(speed: 4, region: "Tag7")
		add_effect(type: "Invincible", duration: 2, period: 1)
		taunt(text: "The souls of fallen warriors empower me!")
		if_elapsed(sec: 1, true: {
			set_texture(index: 4)

			shoot(index: 4, amount: 20, delay: 1.7)
			shoot(index: 5, amount: 16, delay: 1.88)
			shoot(index: 6, amount: 8, delay: 2.06)

			if_elapsed(sec: 2.16, true: {
				spawn(name: "Ascended Soul Crux", rate: 1, max: 2, radius: 0.3, arc: 45)
				spawn(name: "Ascended Soul Crux", rate: 1, max: 2, radius: 0.3, arc: 225)
				set_state(name: "ascended_state_switch", parent: 1)
			})
		}, false: {
			set_texture(index: 2)
		})
	})
	state(name: "ascended_state_switch", actions: {
		set_texture(index: 1)
		wander(speed: 2, period: 0.3)
		stay_near_spawn(speed: 4, distance: 3, enforce: 1, period: 2)
		set_flash(duration: 4)
		shoot_player(index: 10, amount: 5, angleOffsetMin: 0, angleOffsetMax: 360, periodMin: 0.8, periodMax: 1.2)
		shoot_player(index: 6, amount: 8, angleOffsetMin: 0, angleOffsetMax: 360, periodMin: 0.8, periodMax: 1.2)
		if_elapsed(sec: 4, true: {
			random_state(name: "ascended_waves", name: "ascended_blasts", parent: 1)
		})
	})
	state(name: "ascended_waves", actions: {
		shoot_player(index: 9, amount: 5, angleOffsetMin: 0, angleOffsetMax: 360, periodMin: 0.9, periodMax: 1.3)
		state(name: "wander", actions: {
			wander(speed: 2, period: 0.3)
			stay_near_spawn(speed: 4, distance: 3, enforce: 1, period: 2)
			if_elapsed(secMin: 3, secMax: 4, true: {
				set_state(name: "chase", parent: 1)
			})
		})
		state(name: "chase", actions: {
			chase(speed: 4, min: 1, searchRadius: 12)

			shoot_player(index: 5, amount: 9, angleGap: 15, period: 2, delay: 0.4)
			shoot_player(index: 5, amount: 9, angleGap: 15, period: 2, delay: 0.7)
			shoot_player(index: 5, amount: 9, angleGap: 15, period: 2, delay: 1)

			if_elapsed(sec: 5, true: {
				set_state(name: "wander", parent: 1)
			})
		})
		if_elapsed(sec: 10, true: {
			set_state(name: "ascended_state_switch", parent: 1)
		})
	})
	state(name: "ascended_blasts", actions: {
		wander(speed: 2, period: 0.3)
		stay_near_spawn(speed: 4, distance: 3, enforce: 1, period: 2)
		shoot(index: 7, amount: 9, angleGap: 5, angle: 45, period: 1.8)
		shoot(index: 7, amount: 9, angleGap: 5, angle: 135, period: 1.8)
		shoot(index: 7, amount: 9, angleGap: 5, angle: 225, period: 1.8)
		shoot(index: 7, amount: 9, angleGap: 5, angle: 315, period: 1.8)
		shoot(index: 7, amount: 9, angleGap: 5, angle: 0, period: 1.8, delay: 0.9)
		shoot(index: 7, amount: 9, angleGap: 5, angle: 90, period: 1.8, delay: 0.9)
		shoot(index: 7, amount: 9, angleGap: 5, angle: 180, period: 1.8, delay: 0.9)
		shoot(index: 7, amount: 9, angleGap: 5, angle: 270, period: 1.8, delay: 0.9)
		shoot_player(amount: 4, index: 6, angleOffset: 45, periodMin: 0.6, periodMax: 1.5)
		if_elapsed(sec: 10, true: {
			set_state(name: "ascended_state_switch", parent: 1)
		})
	})
	state(name: "pre_rage", actions: {
		taunt(text: "Your end is near!")
		add_effect(type: "Invulnerable", duration: 4)
		set_size(value: 1.4)
		set_flash(color: "red", duration: 10)
		if_elapsed(sec: 4, true: {
			set_state(name: "rage", parent: 1)
		})
	})
	state(name: "rage", actions: {
		chase(speed: 3, min: 1, searchRadius: 12)
		wander(speed: 3, period: 0.3)

		shoot_player(index: 5, amount: 7, angleGap: 15, period: 2.3, delay: 0.4)
		shoot_player(index: 5, amount: 7, angleGap: 15, period: 2.3, delay: 0.8)
		shoot_player(index: 5, amount: 7, angleGap: 15, period: 2.3, delay: 1.2)

		shoot(index: 7, amount: 9, angleGap: 5, angle: 45, period: 3)
		shoot(index: 7, amount: 9, angleGap: 5, angle: 135, period: 3)
		shoot(index: 7, amount: 9, angleGap: 5, angle: 225, period: 3)
		shoot(index: 7, amount: 9, angleGap: 5, angle: 315, period: 3)
		shoot(index: 7, amount: 9, angleGap: 5, angle: 0, period: 3, delay: 1.5)
		shoot(index: 7, amount: 9, angleGap: 5, angle: 90, period: 3, delay: 1.5)
		shoot(index: 7, amount: 9, angleGap: 5, angle: 180, period: 3, delay: 1.5)
		shoot(index: 7, amount: 9, angleGap: 5, angle: 270, period: 3, delay: 1.5)
		
		shoot_player(index: 9, amount: 6, angleOffsetMin: 0, angleOffsetMax: 360, periodMin: 0.9, periodMax: 1.3)
		shoot_player(index: 10, amount: 4, angleOffsetMin: 0, angleOffsetMax: 360, periodMin: 0.9, periodMax: 1.3)
	})
}, death: {
	run_logic_method(name: "spawn_ascension_table")
})
entity_state(name: "Soul Crux", actions: {
	shoot_player(index: 1, periodMin: 0.5, periodMax: 1.6)
	if_leader_state(name: "blades_no_rotation", true: {
		shoot(index: 0, amount: 4, angle: 45, period: 0.36, delay: 0)
		orbit_leader(speed: 5, radius: 6, ignoreCollision: "yes")
	}, false: {
		shoot(index: 0, amount: 4, angle: 0, period: 0.32, delay: 0)
		orbit_leader(speed: 7, radius: 9, ignoreCollision: "yes")
	})
})
entity_state(name: "Ascended Soul Crux", actions: {
	shoot(index: 1, amount: 4, angle: 45, period: 0.3, delay: 1)
	orbit_leader(speed: 4, radius: 9, ignoreCollision: "yes")
	if_leader_dead(true: {
		if_elapsed(sec: 1, true: {
			despawn()
		})
	})
})