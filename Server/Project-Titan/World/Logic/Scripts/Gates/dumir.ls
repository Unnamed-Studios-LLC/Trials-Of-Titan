entity_state(name: "Oda", actions: {
	state(name: "passive_walk", actions: {
		wander(speed: 4, period: 0)
		if_elapsed(secMin: 2, secMax: 4, true: {
			set_state(name: "passive_wait", parent: 1)
		})
		if_health_below(percent: 0.95, true: {
			set_state(name: "upshoot", parent: 1)
		})
	})
	state(name: "passive_wait", actions: {
		stay_near_spawn(speed: 4, distance: 8, enforce: 4, period: 6)
		if_elapsed(secMin: 2, secMax: 4, true: {
			set_state(name: "passive_walk", parent: 1)
		})
		if_health_below(percent: 0.95, true: {
			set_state(name: "upshoot", parent: 1)
		})
	})
	state(name: "upshoot", actions: {
		set_texture(index: 1)
		shoot_player(index: 0, amount: 5, angleGap: 72, period: 1, delay: 0)
		shoot_player(index: 1, amount: 2, angleGap: 0, angleOffset: 0, period: 1, delay: 0)
		shoot_player(index: 1, amount: 2, angleGap: 0, angleOffset: 72, period: 1, delay: 0)
		shoot_player(index: 1, amount: 2, angleGap: 0, angleOffset: 144, period: 1, delay: 0)
		shoot_player(index: 1, amount: 2, angleGap: 0, angleOffset: 216, period: 1, delay: 0)
		shoot_player(index: 1, amount: 2, angleGap: 0, angleOffset: 288, period: 1, delay: 0)
		if_elapsed(secMin: 4, secMax: 6, true: {
			set_state(name: "wander", parent: 1)
		})
	})
	state(name: "wander", actions: {
		set_texture(index: 0)
		stay_near_spawn(speed: 6, distance: 8, enforce: 4, period: 6)
		wander(speed: 4, period: 0.4)
		shoot_player(index: 0, amount: 16, angleGap: 18, periodMin: 1, periodMax: 1.3)
		if_elapsed(secMin: 6, secMax: 9, true: {
			set_state(name: "upshoot", parent: 1)
		})
	})
})
entity_state(name: "Beorn", actions: {
	state(name: "passive_walk", actions: {
		add_effect(type: "Invincible", duration: 2, period: 1.5)
		wander(speed: 4, period: 0)
		if_elapsed(secMin: 2, secMax: 4, true: {
			set_state(name: "passive_wait", parent: 1)
		})
		if_leader_state(name: "passive_walk", false: {
			if_leader_state(name: "passive_wait", false: {
				set_state(name: "wander", parent: 1)
			})
		})
		if_leader_dead(true: {
			set_state(name: "wander", parent: 1)
		})
	})
	state(name: "passive_wait", actions: {
		add_effect(type: "Invincible", duration: 2, period: 1.5)
		stay_near_spawn(speed: 4, distance: 8, enforce: 4, period: 6)
		if_elapsed(secMin: 2, secMax: 4, true: {
			set_state(name: "passive_walk", parent: 1)
		})
		if_leader_state(name: "passive_walk", false: {
			if_leader_state(name: "passive_wait", false: {
				set_state(name: "wander", parent: 1)
			})
		})
		if_leader_dead(true: {
			set_state(name: "wander", parent: 1)
		})
	})
	state(name: "wander", actions: {
		set_flash(duration: 0)
		set_texture(index: 0)
		stay_near_spawn(speed: 6, distance: 8, enforce: 4, period: 6)
		wander(speed: 4, period: 0.4)
		shoot_player(index: 0, amount: 16, angleGap: 18, periodMin: 1, periodMax: 1.3)
		if_elapsed(secMin: 6, secMax: 9, true: {
			set_state(name: "chase", parent: 1)
		})
	})
	state(name: "chase", actions: {
		state(name: "slow", actions: {
			set_texture(index: 1)
			chase(speed: 3, min: 2)
			set_flash(duration: 1.9)
			charge(offset: 2, delay: 1.6, speed: 0.4)
			if_elapsed(sec: 2, true: {
				set_texture(index: 2)
				set_state(name: "charge", parent: 1)
			})
		})
		state(name: "charge", actions: {
			shoot_player(index: 1, amount: 16, angleGap: 18)
			if_elapsed(sec: 0.5, true: {
				set_state(name: "slow", parent: 1)
			})
		})
		if_elapsed(secMin: 5, secMax: 8, true: {
			set_state(name: "wander", parent: 1)
		})
	})
}, death: {
	run_logic_method(name: "spawn_bridge_1")
})
entity_state(name: "Yolma", actions: {
	state(name: "passive_walk", actions: {
		wander(speed: 4, period: 0)
		if_elapsed(secMin: 2, secMax: 4, true: {
			set_state(name: "passive_wait", parent: 1)
		})
		if_health_below(percent: 0.95, true: {
			set_state(name: "activate", parent: 1)
		})
	})
	state(name: "passive_wait", actions: {
		stay_near_spawn(speed: 4, distance: 8, enforce: 4, period: 6)
		if_elapsed(secMin: 2, secMax: 4, true: {
			set_state(name: "passive_walk", parent: 1)
		})
		if_health_below(percent: 0.95, true: {
			set_state(name: "activate", parent: 1)
		})
	})
	state(name: "activate", actions: {
		add_effect(type: "Invulnerable", duration: 2, period: 1.5)
		move_to_region(region: "Shop2", speed: 5)
		if_elapsed(sec: 2, true: {
			spawn(name: "Yolma's Orb", rate: 1, max: 4, radius: 1, arc: 0)
			spawn(name: "Yolma's Orb", rate: 1, max: 4, radius: 1, arc: 90)
			spawn(name: "Yolma's Orb", rate: 1, max: 4, radius: 1, arc: 180)
			spawn(name: "Yolma's Orb", rate: 1, max: 4, radius: 1, arc: 270)
			set_state(name: "switch", parent: 1)
		})
	})
    state(name: "switch", actions: {
		add_effect(type: "Fortified", duration: 2, period: 1.5)
        wander(speed: 2, period: 0.5)
		stay_near_spawn(speed: 3, distance: 3, enforce: 2, period: 3, delay: 2)
        if_elapsed(sec: 4, true: {
            random_state(name: "shoot_sin", name: "shoot_scatter", parent: 1)
        })
    })
    state(name: "shoot_sin", actions: {
		move_to_region(region: "Shop2", speed: 3)
        shoot_player(index: 1, amount: 15, period: 1, delay: 0.4)
        shoot_player(index: 2, amount: 2, periodMin: 1, periodMax: 1.4)
        if_elapsed(sec: 9, true: {
            set_state(name: "switch", parent: 1)
        })
    })
    state(name: "shoot_scatter", actions: {
		move_to_region(region: "Shop2", speed: 3)
        shoot(index: 0, amount: 6, angle: 0, period: 1, delay: 0)
        shoot(index: 0, amount: 6, angle: 50, period: 1, delay: 0.18)
        shoot(index: 0, amount: 6, angle: 30, period: 1, delay: 0.36)
        shoot(index: 0, amount: 6, angle: 40, period: 1, delay: 0.54)
        shoot(index: 0, amount: 6, angle: 20, period: 1, delay: 0.72)
        shoot(index: 0, amount: 5, angle: 10, period: 1, delay: 0.9)
        if_elapsed(sec: 9, true: {
            set_state(name: "switch", parent: 1)
        })
    })
})
entity_state(name: "Yolma's Orb", actions: {
	orbit_leader(speed: 3, radius: 8, ignoreCollision: "yes")
	shoot_leader(angleOffset: 50, period: 0.3, delay: 3)
	shoot_leader(angleOffset: -50, period: 0.3, delay: 3)
	if_leader_dead(true: {
		if_elapsed(sec: 2, true: {
			despawn()
		})
	})
})
entity_state(name: "Freagun Axe Thrower", actions: {
	wander(speed: 4, period: 1)
	chase(speed: 4, min: 1.5)
	shoot_player(amount: 1, angleGap: 0, delay: 0, period: 2)
	shoot_player(amount: 2, angleGap: 5, delay: 0.2, period: 2)
	shoot_player(amount: 2, angleGap: 10, delay: 0.4, period: 2)
	shoot_player(amount: 2, angleGap: 15, delay: 0.6, period: 2)
	shoot_player(amount: 2, angleGap: 20, delay: 0.8, period: 2)
	shoot_player(amount: 2, angleGap: 25, delay: 1.0, period: 2)
	shoot_player(amount: 2, angleGap: 30, delay: 1.2, period: 2)
	shoot_player(amount: 2, angleGap: 35, delay: 1.4, period: 2)
})
entity_state(name: "Freagun Bowman", actions: {
	wander(speed: 5, period: 1)
	chase(speed: 2, min: 5)
	shoot_player(amount: 3, angleGap: 20, periodMin: 1.2, periodMax: 1.6)
})
entity_state(name: "Jyorgin Mammoth", actions: {
	stay_near_spawn(speed: 8, distance: 16, enforce: 4, period: 8)
	state(name: "wander", actions: {
		wander(speed: 3, period: 0.8)
		set_texture(index: 0)
		if_elapsed(sec: 3.6, true: {
			if_player_within(distance: 10, true: {
				set_state(name: "prep", parent: 1)
			})
		})
	})
	state(name: "prep", actions: {
		set_texture(index: 1)
		charge(offset: 2.5, delay: 0.6, speed: 0.6, searchRadius: 10)
		set_flash(duration: 0.8)
		if_elapsed(sec: 1.2, true: {
			set_texture(index: 2)
			set_state(name: "charge", parent: 1)
		})
	})
	state(name: "charge", actions: {
		shoot_player(index: 0, amount: 6, angleGap: 15, delay: 0.1)
		if_elapsed(sec: 0.4, true: {
			set_state(name: "wander", parent: 1)
		})
	})
})
entity_state(name: "Jyorgin Warrior", actions: {
	wander(speed: 3, period: 0.6)
	chase(speed: 6, min: 1.5)
	shoot_player(amount: 6, angleGap: 25, angleOffsetMin: -15, angleOffsetMax: 15, periodMin: 1.2, periodMax: 1.5)
})
entity_state(name: "Recluse Dumirian", actions: {
	wander(speed: 8, period: 4)
	shoot_player(amount: 8, periodMin: 2.2, periodMax: 3.4)
	shoot_player(amount: 1, periodMin: 0.4, periodMax: 0.8)
})
entity_state(name: "Raeg", actions: {
	state(name: "passive_walk", actions: {
		wander(speed: 4, period: 0)
		if_elapsed(secMin: 2, secMax: 4, true: {
			set_state(name: "passive_wait", parent: 1)
		})
		if_health_below(percent: 0.95, true: {
			set_state(name: "activate", parent: 1)
		})
	})
	state(name: "passive_wait", actions: {
		stay_near_spawn(speed: 4, distance: 8, enforce: 4, period: 6)
		if_elapsed(secMin: 2, secMax: 4, true: {
			set_state(name: "passive_walk", parent: 1)
		})
		if_health_below(percent: 0.95, true: {
			set_state(name: "activate", parent: 1)
		})
	})
	state(name: "activate", actions: {
		set_flash(duration: 0)
		set_texture(index: 0)
		stay_near_spawn(speed: 6, distance: 8, enforce: 4, period: 6)
		wander(speed: 4, period: 0.4)
		shoot_player(index: 0, amount: 10, angleGap: 18, period: 0.8)
		shoot_player(index: 0, amount: 6, angleGap: 18, period: 0.8, angleOffset: 180)
		if_elapsed(secMin: 6, secMax: 8, true: {
			random_state(name: "still_shoot", name: "move_shoot", parent: 1)
		})
	})
	state(name: "still_shoot", actions: {
		stay_near_spawn(speed: 6, distance: 1, enforce: 4, period: 1)
		shoot(amount: 2, angle: 0, delay: 0.2)
		shoot(amount: 2, angle: 180, delay: 0.2)
		shoot(amount: 2, angle: 0, delay: 0.4)
		shoot(amount: 2, angle: 180, delay: 0.4)
		shoot(amount: 2, angle: 0, delay: 0.6)
		shoot(amount: 2, angle: 180, delay: 0.6)
		shoot(amount: 2, angle: 0, delay: 0.8)
		shoot(amount: 2, angle: 180, delay: 0.8)
		shoot_spiral(amount: 2, angle: 0, angleStep: 10, period: 0.18, delay: 1)
		shoot_spiral(amount: 2, angle: 90, angleStep: -10, period: 0.18, delay: 1)
		if_elapsed(sec: 8, true: {
			set_state(name: "activate", parent: 1)
		})
	})
	state(name: "move_shoot", actions: {
		stay_near_spawn(speed: 5, distance: 3, enforce: 2, period: 3)
		wander(speed: 2, period: 0.6)
		shoot_player(amount: 6, angleGap: 15, delay: 0.4, period: 1)
		shoot_player(amount: 5, angleGap: 15, delay: 0.8, period: 1)
		shoot(index: 1, amount: 8, angleMin: 0, angleMax: 360, period: 1, delay: 0.2)
		if_elapsed(sec: 8, true: {
			set_state(name: "activate", parent: 1)
		})
	})
}, death: {
	run_logic_method(name: "spawn_bridge_2")
})
entity_state(name: "Bhaling Swordsman", actions: {
    wander(speed: 2, period: 0.4)
    chase(speed: 5, min: 2)
    state(name: "start", actions: {
        random_state(name: "orbit_cw", name: "oribt_ccw", parent: 1)
    })
    state(name: "orbit_ccw", actions: {
        orbit_player(speed: 5, radius: 3, searchRadius: 10)
    }) 
    state(name: "orbit_cw", actions: {
        orbit_player(speed: 5, radius: 3, searchRadius: 10)
    })
    shoot_player(amount: 5, period: 2)
})
entity_state(name: "Bhaling Axeman", actions: {
    wander(speed: 2, period: 0.7)
    chase(speed: 4, min: 2)
    state(name: "start", actions: {
        random_state(name: "shoot_l", name: "shoot_r", parent: 1)
    })
    state(name: "shoot_l", actions: {
        shoot_player(index: 0, period: 1.2, delay: 0, angleOffset: 180)
        shoot_player(index: 1, period: 1.2, delay: 0, angleOffset: 180)
        shoot_player(index: 2, period: 1.2, delay: 0, angleOffset: 180)
        shoot_player(index: 3, period: 1.2, delay: 0, angleOffset: 180)
    }) 
    state(name: "shoot_r", actions: {
        shoot_player(index: 4, period: 1.2, delay: 0)
        shoot_player(index: 5, period: 1.2, delay: 0)
        shoot_player(index: 6, period: 1.2, delay: 0)
        shoot_player(index: 7, period: 1.2, delay: 0)
    })
})