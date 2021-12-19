entity_state(name: "Orc Beastmaster", actions: {
	spawn(name: "Worgle Chain", rate: 3, max: 3)
	wander(speed: 4, period: 0.6)
	shoot_player(index: 0, amount: 14, periodMin: 1.2, periodMax: 1.6)
}, death: {
	chain_kill(name: "Worgle Chain")
})
entity_state(name: "Worgle Chain", actions: {
	state(name: "spawn", actions: {
		if_minion(leaderCount: 3, false: {
			spawn(name: "Worgle Chain", rate: 1, max: 1, radius: 0)
		}, true: {
			spawn(name: "Worgle", rate: 1, max: 1, radius: 3)
		})
		set_state(name: "link", parent: 1)
	})
	state(name: "link", actions: {
		linked_part()
	})
	wander(speed: 3, period: 0.6)
}, death: {
	chain_kill(name: "Worgle Chain")
})
entity_state(name: "Worgle", actions: {
	wander(speed: 3, period: 0.4)
	chase(speed: 1, min: 1)
	hover_leader_chase(speed: 5, minHover: 2, maxHover: 4, chaseDuration: 5, min: 1)
	shoot_player(index: 0, amount: 5, angleGap: 30, period: 1.2)
}, death: {
	chain_kill(name: "Worgle Chain")
})
entity_state(name: "Orc Bladesman", actions: {
	wander(speed: 4, period: 0.4)
	chase(speed: 4, min: 3)
	shoot_player(amount: 5, angleGap: 24, periodMin: 0.5, periodMax: 1.2)
})
entity_state(name: "Orc Warrior", actions: {
	wander(speed: 4, period: 0.4)
	chase(speed: 4, min: 5)
	shoot_player(index: 0, period: 0.4, angleOffset: 180)
	shoot_player(index: 1, period: 0.4)
})
entity_state(name: "Worgle Rider", actions: {
	wander(speed: 5, period: 0.2)
	chase(speed: 6, min: 3)
	shoot_player(index: 0, amount: 2, angleGap: 0, period: 0.8)
	shoot_player(index: 1, amount: 3, angleGap: 40, periodMin: 0.8, periodMax: 1.3)
})
entity_state(name: "Orc Bruiser", actions: {
	state(name: "wander", actions: {
		wander(speed: 5, period: 0.2)
		if_elapsed(sec: 2, true: {
			if_player_within(distance: 8, true: {
				set_state(name: "fight", parent: 1)
			})
		})
	})
	state(name: "fight", actions: {
		wander(speed: 2, period: 0.8)
		chase(speed: 10, min: 5)
		if_elapsed(sec: 3, true: {
			if_player_within(distance: 8, true: {
				set_state(name: "wander", parent: 1)
			})
		})
		shoot_player(amount: 10, angleGap: 18, periodMin: 0.8, periodMax: 1.2)
	})
})
entity_state(name: "Bothmur", actions: {
	if_health_below(percent: 0.15, trigger: "yes", true: {
		set_state(name: "rage")
	})
	state(name: "undisturbed", actions: {
		wander(speed: 0.5, period: 1)
		if_health_below(percent: 0.95, true: {
			set_state(name: "begin", parent: 1)
		})
	})
	state(name: "begin", actions: {
		shoot(index: 4, amount: 18)
		shoot(index: 4, amount: 18, delay: 0.4)
		if_elapsed(sec: 0.6, true: {
			random_state(name: "stab", parent: 1)
		})
	})
	state(name: "stab", actions: {
		state(name: "chase", actions: {
			set_flash(duration: 3)
			chase(speed: 4, min: 2)
			shoot_player(index: 0, amount: 9, angleGap: 15, period: 1.2, delay: 0.4)
			shoot_player(index: 0, amount: 8, angleGap: 15, period: 1.2, delay: 1)
			if_elapsed(sec: 3, true: {
				shoot(index: 4, amount: 18)
				set_state(name: "wait", parent: 1)
			})
		})
		state(name: "wait", actions: {
			set_flash(duration: 0)
			wander(speed: 2, period: 0.5)
			shoot_player(index: 1, amount: 4, angleGap: 15, period: 1, delay: 0.3)
			shoot_player(index: 1, amount: 5, angleGap: 15, period: 1, delay: 0.8)
			if_elapsed(sec: 2, true: {
				shoot(index: 4, amount: 18)
				set_state(name: "chase", parent: 1)
			})
		})
		if_elapsed(sec: 10.2, true: {
			set_flash(duration: 0)
			shoot(index: 4, amount: 18)
			set_state(name: "wave", parent: 1)
		})
	})
	state(name: "wave", actions: {
		if_elapsed(sec: 10, true: {
			shoot(index: 4, amount: 18)
			set_state(name: "stab", parent: 1)
		})
	})
})
entity_state(name: "Forge Foundry", actions: {
	if_health_below(percent: 0.9, trigger: "yes", true: {
		spawn(name: "Forge Pillar", rate: 1, max: 4, arc: 0, radius: 7)
		spawn(name: "Forge Pillar", rate: 1, max: 4, arc: 90, radius: 7)
		spawn(name: "Forge Pillar", rate: 1, max: 4, arc: 180, radius: 7)
		spawn(name: "Forge Pillar", rate: 1, max: 4, arc: 270, radius: 7)
	})
	state(name: "shoot", actions: {
		shoot_player(period: 1.1, amount: 5, angleGap: 20, delay: 0.5)
		if_elapsed(sec: 10, true: {
			set_state(name: "lava", parent: 1)
		})
	})
	state(name: "lava", actions: {
		shoot_player(period: 1.1, amount: 5, angleGap: 20, delay: 0.5)
		crack_ground(tile: "Valdok's Lava", joints: 2, jointAngleMin: -20, jointAngleMax: 20, rate: 4, radius: 12)
		crack_ground(tile: "Valdok's Lava", joints: 2, jointAngleMin: -20, jointAngleMax: 20, rate: 4, radius: 12)
		crack_ground(tile: "Valdok's Lava", joints: 2, jointAngleMin: -20, jointAngleMax: 20, rate: 4, radius: 12)
		crack_ground(tile: "Valdok's Lava", joints: 2, jointAngleMin: -20, jointAngleMax: 20, rate: 4, radius: 12)
		crack_ground(tile: "Valdok's Lava", joints: 2, jointAngleMin: -20, jointAngleMax: 20, rate: 4, radius: 12)
		if_elapsed(sec: 3.5, true: {
			set_state(name: "shoot", parent: 1)
		})
	})
})
entity_state(name: "Forge Pillar", actions: {
	if_leader_dead(true: {
		if_elapsed(sec: 3, true: {
			despawn()
		})
	})
	state(name: "normal", actions: {
		shoot(amount: 4, angle: 45, period: 0.25, delay: 0.5)
		if_elapsed(sec: 8, true: {
			set_state(name: "diagonal", parent: 1)
		})
	})
	state(name: "diagonal", actions: {
		shoot(amount: 4, angle: 0, period: 0.25, delay: 0.5)
		if_elapsed(sec: 8, true: {
			set_state(name: "normal", parent: 1)
		})
	})
})