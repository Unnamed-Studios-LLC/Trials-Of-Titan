entity_state(name: "Ancient Bone Worm", actions: {
	spawn(name: "Ancient Bone Worm Body", rate: 1, max: 1, radius: 0.2, arc: 181)

	state(name: "inactive", actions: {
		set_texture(index: 1)
		add_effect(type: "Invincible", duration: 2, period: 1)
		orbit_region(region: "Tag2", radius: 14, speed: 6, ignoreCollision: "yes")

		if_player_within(distance: 18, true: {
			if_elapsed(sec: 4, true: {
				set_state(name: "unearth", parent: 1)
			})
		})
	})

	state(name: "unearth", actions: {
		set_texture(index: 2)
		orbit_region(region: "Tag2", radius: 14, speed: 6, ignoreCollision: "yes")
		shoot_region(region: "Tag3", index: 0, amount: 4, angle: 45, period: 0.3)
		if_elapsed(sec: 0.75, true: {
			set_state(name: "circle", parent: 1)
		})
	})

	state(name: "circle", actions: {
		set_texture(index: 0)
		orbit_region(region: "Tag2", radius: 14, speed: 6, ignoreCollision: "yes")
		shoot_region(region: "Tag3", index: 0, amount: 4, angle: 45, period: 0.3, delay: 0.15)
		shoot_player(index: 2, amount: 5, angleGap: 20, period: 1)

		if_elapsed(sec: 8, true: {
			set_state(name: "burrow_to_center", parent: 1)
		})
	})

	state(name: "burrow_to_center", actions: {
		set_texture(index: 1)
		add_effect(type: "Invincible", duration: 2, period: 1)
		move_to_region(region: "Tag2", speed: 8)

		if_elapsed(sec: 3, true: {
			set_state(name: "center_shoot", parent: 1)
		})
	})

	state(name: "center_shoot", actions: {
		if_elapsed(sec: 0.75, false: {
			add_effect(type: "Invincible", duration: 0.75)
			set_texture(index: 2)
		}, true: {
			set_texture(index: 0)
		})

		shoot(index: 3, angle: 0, amount: 6, period: 1, delay: 0.75)
		shoot_player(index: 2, amount: 5, angleGap: 20, period: 1, delay: 1.25)

		if_elapsed(sec: 10, true: {
			set_state(name: "unearth", parent: 1)
		})
	})
}, death: {
	run_logic_method(name: "remove_bhognin_doors")
})
entity_state(name: "Ancient Bone Worm Body", actions: {
	if_elapsed(sec: 1, false: {
		if_minion(leaderCount: 10, false: {
			spawn(name: "Ancient Bone Worm Body", rate: 1, max: 1, radius: 0.8, arc: 240, minionArcOffset: 24, minionArcExp: 0.1)
		})
	})
	if_leader_dead(false: {
		spring_chase_leader(distance: 1, acceleration: 3, drag: 1, velocityMax: 10, ignoreCollision: "yes")
		if_leader_state(name: "burrow", name: "inactive", name: "burrow_to_center", name: "center_shoot", true: {
			add_effect(type: "Invincible", duration: 2, period: 1)
			set_texture(index: 1)
			if_elapsed(sec: 0.15, true: {
				set_state(name: "burrow", parent: 0)
			})
		}, false: {
			if_leader_state(name: "unearth", true: {
				set_texture(index: 2)
				if_elapsed(sec: 0.15, true: {
					set_state(name: "unearth", parent: 0)
				})
			}, false: {
				shoot(amount: 6, period: 1, delayMin: 0, delayMax: 1)
				set_texture(index: 0)
				if_elapsed(sec: 0.15, true: {
					set_state(name: "fight", parent: 0)
				})
			})
		})
	}, true: {
		wander(speed: 4, period: 0.6)
		if_elapsed(sec: 0.75, true: {
			set_state(name: "fight", parent: 0)
			set_texture(index: 0)
		}, false: {
			set_state(name: "unearth", parent: 0)
			set_texture(index: 2)
		})
	})
}, death: {
	give_minions_to_leader()
})
entity_state(name: "Falling Rock", actions: {
	shoot(amount: 4, angle: 45, periodMin: 3, periodMax: 5)
})
entity_state(name: "Barrens Golem", actions: {
	state(name: "chase", actions: {
		wander(speed: 1, period: 0.3)
		chase(speed: 3)
		if_elapsed(sec: 3, true: {
			set_state(name: "shoot", parent: 1)
		})
	})
	state(name: "shoot", actions: {
		shoot(amount: 6, angle: 30, period: 0.6)
		shoot(amount: 6, angle: 0, period: 0.6, delay: 0.3)
		if_elapsed(sec: 4, true: {
			set_state(name: "chase", parent: 1)
		})
	})
})
entity_state(name: "Barren Vulture", actions: {
	wander(speed: 1.5, period: 0.3)
	state(name: "chase", actions: {
		chase(speed: 6, min: 5, searchRadius: 12)
		shoot_player(amount: 2, angleGap: 0, period: 1)
		if_elapsed(sec: 4, true: {
			set_state(name: "flee", parent: 1)
		})
	})
	state(name: "flee", actions: {
		flee(speed: 6, max: 8)
		if_elapsed(sec: 3, true: {
			set_state(name: "chase", parent: 1)
		})
	})
})
entity_state(name: "Barrens Iron Golem", actions: {
	state(name: "start", actions: {
		spawn(name: "Barrens Iron Rock", rate: 1, max: 2, arc: 0, radius: 1)
		spawn(name: "Barrens Iron Rock", rate: 1, max: 2, arc: 180, radius: 1)
		set_state(name: "chase", parent: 1)
	})
	state(name: "chase", actions: {
		wander(speed: 1, period: 0.3)
		chase(speed: 4)
		if_elapsed(sec: 3, true: {
			set_state(name: "shoot", parent: 1)
		})
	})
	state(name: "shoot", actions: {
		chase(speed: 1)
		shoot_player(amount: 1, angleGap: 0, delay: 0)
		shoot_player(amount: 2, angleGap: 10, delay: 0.15)
		shoot_player(amount: 2, angleGap: 20, delay: 0.30)
		shoot_player(amount: 2, angleGap: 30, delay: 0.45)
		shoot_player(amount: 2, angleGap: 40, delay: 0.60)
		shoot_player(amount: 2, angleGap: 50, delay: 0.75)
		shoot_player(amount: 2, angleGap: 60, delay: 0.90)
		shoot_player(amount: 2, angleGap: 70, delay: 1.05)
		shoot_player(amount: 2, angleGap: 80, delay: 1.20)
		shoot_player(amount: 2, angleGap: 90, delay: 1.35)
		shoot_player(amount: 2, angleGap: 80, delay: 1.50)
		shoot_player(amount: 2, angleGap: 70, delay: 1.65)
		shoot_player(amount: 2, angleGap: 60, delay: 1.80)
		shoot_player(amount: 2, angleGap: 50, delay: 1.95)
		shoot_player(amount: 2, angleGap: 40, delay: 2.10)
		shoot_player(amount: 2, angleGap: 30, delay: 2.25)
		shoot_player(amount: 2, angleGap: 20, delay: 2.40)
		shoot_player(amount: 2, angleGap: 10, delay: 2.55)
		shoot_player(amount: 1, angleGap: 0, delay: 2.70)

		if_elapsed(sec: 3, true: {
			set_state(name: "chase", parent: 1)
		})
	})
})
entity_state(name: "Barrens Iron Rock", actions: {
	orbit_leader(speed: 5, radius: 5, ignoreCollision: "yes")
	shoot(amount: 10, period: 4, delayMin: 0, delayMax: 4)
})
entity_state(name: "Barren Tortoise", actions: {
	state(name: "listening", actions: {
		if_player_within(distance: 10, true: {
			set_state(name: "shoot", parent: 1)
		})
	})
	state(name: "shoot", actions: {
		set_texture(index: 1)
		shoot(amount: 6, angle: 0, period: 2, delay: 0.1)
		shoot(amount: 6, angle: 10, period: 2, delay: 0.2)
		shoot(amount: 6, angle: 20, period: 2, delay: 0.3)

		shoot(amount: 6, angle: 30, period: 2, delay: 1.1)
		shoot(amount: 6, angle: 40, period: 2, delay: 1.2)
		shoot(amount: 6, angle: 50, period: 2, delay: 1.3)

		if_elapsed(sec: 4.4, true: {
			set_state(name: "wander", parent: 1)
		})
	})
	state(name: "wander", actions: {
		set_texture(index: 0)
		wander(speed: 1, period: 2)
		if_elapsed(sec: 4, true: {
			set_state(name: "listening", parent: 1)
		})
	})
})
entity_state(name: "Ravine Raiding Shaman", actions: {
	spawn(name: "Ravine Raiding Archer", rate: 3, max: 3)
	wander(speed: 2, period: 0.6)
	chase(speed: 2, min: 6)
	shoot_player(index: 0, amount: 3, angleGap: 20, period: 1, delay: 0)
	shoot(index: 2, amount: 14, period: 2)
	shoot(index: 1, amount: 6, period: 3)
})
entity_state(name: "Ravine Raiding Archer", actions: {
	wander(speed: 2, period: 0.3)
	hover_leader_chase(speed: 5, min: 4, minHover: 3, maxHover: 5, chaseDuration: 5)
	shoot_player(index: 0, amount: 1, period: 1.6, delay: 0)
	shoot_player(index: 0, amount: 2, angleGap: 30, period: 1.6, delay: 0.8)
})