entity_state(name: "Overworld Rictorn", actions: {
	state(name: "passive", actions: {
		timed_switch(period: 1.8, delay: 1.8, actions: {
			wander(speed: 2)
			do_nothing()
		})
		stay_near_spawn(speed: 4, distance: 5, enforce: 2, period: 4)
		if_health_below(percent: 0.95, true: {
			set_state(name: "pre_fight_2", parent: 1)
		})
	})
	state(name: "pre_fight_2", actions: {
		add_effect(type: "Invincible", duration: 2)
		set_texture(index: 1)
		set_flash(duration: 1.85)
		if_elapsed(sec: 2, true: {
			set_texture(index: 2)
			set_state(name: "fight_2", parent: 1)
		})
	})
	state(name: "fight_2", actions: {
		state(name: "spiral", actions: {
			shoot_spiral(index: 0, amount: 4, angleStep: 7, period: 0.2, delay: 0.5)
			if_elapsed(sec: 8, true: {
				set_state(name: "cooldown", parent: 1)
			})
		})
		state(name: "cooldown", actions: {
			shoot_player(index: 1, amount: 11, period: 0.8)
			if_elapsed(sec: 5, true: {
				set_state(name: "spiral", parent: 1)
			})
		})
		if_health_below(percent: 0.4, true: {
			set_state(name: "pre_fight_3", parent: 1)
		})
	})
	state(name: "pre_fight_3", actions: {
		add_effect(type: "Invincible", duration: 2)
		set_flash(duration: 1.8)
		if_elapsed(sec: 2, true: {
			set_state(name: "fight_3", parent: 1)
		})
	})
	state(name: "fight_3", actions: {
		spawn(name: "Prophet of Rictorn", rate: 1, max: 2, radius: 0.3, arc: 30)
		spawn(name: "Acolyte of Rictorn", rate: 1, max: 2, radius: 0.3, arc: 150)
		spawn(name: "Magus of Rictorn", rate: 1, max: 2, radius: 0.3, arc: 270)
		shoot(index: 0, amount: 14, angleMin: 0, angleMax: 360, period: 4, delay: 0.5)
		stay_near_spawn(speed: 4, distance: 10, enforce: 2, period: 4)
		wander(speed: 2, period: 0.6)
	})
}, death: {
	create_gate(type: "woods")
})
entity_state(name: "Rictorn", actions: {
	state(name: "passive", actions: {
		timed_switch(period: 1.8, delay: 1.8, actions: {
			wander(speed: 2)
			do_nothing()
		})
		stay_near_spawn(speed: 4, distance: 5, enforce: 2, period: 4)
		if_health_below(percent: 0.95, true: {
			set_state(name: "pre_fight_1", parent: 1)
		})
	})
	state(name: "pre_fight_1", actions: {
		add_effect(type: "Invincible", duration: 2, period: 1)
		set_texture(index: 1)
		if_elapsed(sec: 1.85, true: {
			set_state(name: "fight_1", parent: 1)
		})
	})
	state(name: "fight_1", actions: {
		set_texture(index: 2)
		spawn(name: "Prophet of Rictorn", rate: 1, max: 2, radius: 0.3, arc: 30)
		spawn(name: "Acolyte of Rictorn", rate: 1, max: 2, radius: 0.3, arc: 150)
		spawn(name: "Magus of Rictorn", rate: 1, max: 2, radius: 0.3, arc: 270)
		shoot(index: 0, amount: 6, angleMin: 0, angleMax: 360, period: 1, delay: 0)
		shoot_player(index: 1, amount: 2, angleGap: 15, period: 1, angleOffset: 0, delay: 0.5)
		shoot_player(index: 1, amount: 2, angleGap: 15, period: 1, angleOffset: 90, delay: 0.5)
		shoot_player(index: 1, amount: 2, angleGap: 15, period: 1, angleOffset: 180, delay: 0.5)
		shoot_player(index: 1, amount: 2, angleGap: 15, period: 1, angleOffset: 270, delay: 0.5)
		if_health_below(percent: 0.65, true: {
			set_state(name: "pre_fight_2", parent: 1)
		})
	})
	state(name: "pre_fight_2", actions: {
		add_effect(type: "Invincible", duration: 2)
		set_flash(duration: 1.8)
		if_elapsed(sec: 2, true: {
			set_state(name: "fight_2", parent: 1)
		})
	})
	state(name: "fight_2", actions: {
		state(name: "spiral", actions: {
			shoot_spiral(index: 0, amount: 4, angleStep: 7, period: 0.2, delay: 0.5)
			if_elapsed(sec: 8, true: {
				set_state(name: "cooldown", parent: 1)
			})
		})
		state(name: "cooldown", actions: {
			shoot_player(index: 1, amount: 11, period: 0.8)
			if_elapsed(sec: 5, true: {
				set_state(name: "spiral", parent: 1)
			})
		})
		if_health_below(percent: 0.3, true: {
			set_state(name: "pre_fight_3", parent: 1)
		})
	})
	state(name: "pre_fight_3", actions: {
		add_effect(type: "Invincible", duration: 2)
		set_flash(duration: 1.8)
		if_elapsed(sec: 2, true: {
			set_state(name: "fight_3", parent: 1)
		})
	})
	state(name: "fight_3", actions: {
		spawn(name: "Prophet of Rictorn", rate: 1, max: 2, radius: 0.3, arc: 30)
		spawn(name: "Acolyte of Rictorn", rate: 1, max: 2, radius: 0.3, arc: 150)
		spawn(name: "Magus of Rictorn", rate: 1, max: 2, radius: 0.3, arc: 270)
		shoot(index: 0, amount: 14, angleMin: 0, angleMax: 360, period: 4, delay: 0.5)
	})
}, death: {
	create_return_portal()
})
entity_state(name: "Acolyte of Rictorn", actions: {
	state(name: "close", actions: {
		orbit_leader(speed: 6, radius: 5, ignoreCollision: "yes")
		if_elapsed(sec: 4, true: {
			set_state(name: "far", parent: 1)
		})
	})
	state(name: "far", actions: {
		orbit_leader(speed: 4, radius: 7, ignoreCollision: "yes")
		if_elapsed(sec: 4, true: {
			set_state(name: "far", parent: 1)
		})
	})
	shoot(amount: 4, angle: 45, period: 2.4, delay: 0)
	shoot(amount: 4, angle: 0, period: 2.4, delay: 1.2)
	if_leader_state(name: "fight_3", true: {
		heal_leader(amount: 400, maxPercent: 0.3, period: 4, delayMin: 0, delayMax: 4)
	}, false: {
		heal_leader(amount: 400, maxPercent: 0.65, period: 4, delayMin: 0, delayMax: 4)
	})
})
entity_state(name: "Magus of Rictorn", actions: {
	state(name: "close", actions: {
		orbit_leader(speed: 6, radius: 5, ignoreCollision: "yes")
		if_elapsed(sec: 4, true: {
			set_state(name: "far", parent: 1)
		})
	})
	state(name: "far", actions: {
		orbit_leader(speed: 4, radius: 7, ignoreCollision: "yes")
		if_elapsed(sec: 4, true: {
			set_state(name: "far", parent: 1)
		})
	})
	shoot(amount: 5, angleMin: 0, angleMax: 360, periodMin: 1, periodMax: 1.5)
	if_leader_state(name: "fight_3", true: {
		heal_leader(amount: 400, maxPercent: 0.3, period: 4, delayMin: 0, delayMax: 4)
	}, false: {
		heal_leader(amount: 400, maxPercent: 0.65, period: 4, delayMin: 0, delayMax: 4)
	})
})
entity_state(name: "Prophet of Rictorn", actions: {
	state(name: "close", actions: {
		orbit_leader(speed: 6, radius: 5, ignoreCollision: "yes")
		if_elapsed(sec: 4, true: {
			set_state(name: "far", parent: 1)
		})
	})
	state(name: "far", actions: {
		orbit_leader(speed: 4, radius: 7, ignoreCollision: "yes")
		if_elapsed(sec: 4, true: {
			set_state(name: "far", parent: 1)
		})
	})
	shoot(amount: 5, angleMin: 0, angleMax: 360, periodMin: 1.4, periodMax: 1.8)
	if_leader_state(name: "fight_3", true: {
		heal_leader(amount: 400, maxPercent: 0.3, period: 4, delayMin: 0, delayMax: 4)
	}, false: {
		heal_leader(amount: 400, maxPercent: 0.65, period: 4, delayMin: 0, delayMax: 4)
	})
})