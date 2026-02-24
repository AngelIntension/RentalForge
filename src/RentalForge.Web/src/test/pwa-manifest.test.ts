// @vitest-environment node
import { describe, it, expect } from 'vitest'
import { pwaManifest } from '../../vite.config'

describe('PWA manifest', () => {
  it('has correct name', () => {
    expect(pwaManifest.name).toBe('RentalForge')
  })

  it('has correct short_name', () => {
    expect(pwaManifest.short_name).toBe('RentalForge')
  })

  it('has start_url set to root', () => {
    expect(pwaManifest.start_url).toBe('/')
  })

  it('has standalone display mode', () => {
    expect(pwaManifest.display).toBe('standalone')
  })

  it('has 192x192 icon', () => {
    const icon192 = pwaManifest.icons.find(
      (i) => i.sizes === '192x192' && !i.purpose,
    )
    expect(icon192).toBeDefined()
  })

  it('has 512x512 icon', () => {
    const icon512 = pwaManifest.icons.find(
      (i) => i.sizes === '512x512' && !i.purpose,
    )
    expect(icon512).toBeDefined()
  })

  it('has maskable 192x192 icon', () => {
    const maskable192 = pwaManifest.icons.find(
      (i) => i.sizes === '192x192' && i.purpose === 'maskable',
    )
    expect(maskable192).toBeDefined()
  })

  it('has maskable 512x512 icon', () => {
    const maskable512 = pwaManifest.icons.find(
      (i) => i.sizes === '512x512' && i.purpose === 'maskable',
    )
    expect(maskable512).toBeDefined()
  })
})
